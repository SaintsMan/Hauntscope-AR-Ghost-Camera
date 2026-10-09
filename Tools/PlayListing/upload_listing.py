import argparse
import hashlib
import os
import sys
import urllib.request
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parent.parent
sys.path.insert(0, str(HERE.parent / 'PlayIap'))
from play_iap import Play, PlayError

META = ROOT / 'fastlane' / 'metadata' / 'android'
UPLOAD = 'https://androidpublisher.googleapis.com/upload/androidpublisher/v3/applications/'
LIMITS = {'title': 30, 'shortDescription': 80, 'fullDescription': 4000}
IMAGE_TYPES = [('icon', 'icon'), ('featureGraphic', 'featureGraphic'), ('phoneScreenshots', 'phoneScreenshots')]
IMAGE_SUFFIXES = {'.png': 'image/png', '.jpg': 'image/jpeg', '.jpeg': 'image/jpeg'}


def images_at(folder, name):
    directory = folder / name
    if directory.is_dir():
        return sorted(p for p in directory.iterdir() if p.suffix.lower() in IMAGE_SUFFIXES)
    return [p for p in (folder / (name + suffix) for suffix in IMAGE_SUFFIXES) if p.exists()][:1]


def local_listing(lang):
    folder = META / lang
    text = lambda name: (folder / name).read_text(encoding='utf-8').strip()
    listing = {'language': lang, 'title': text('title.txt'), 'shortDescription': text('short_description.txt'),
               'fullDescription': text('full_description.txt')}
    images = {kind: images_at(folder / 'images', name) for kind, name in IMAGE_TYPES}
    return listing, images


def sha256(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def upload(play, edit, lang, kind, path):
    url = UPLOAD + play.package + '/edits/%s/listings/%s/%s?uploadType=media' % (edit, lang, kind)
    request = urllib.request.Request(url, data=path.read_bytes(), method='POST')
    request.add_header('Authorization', 'Bearer ' + play._auth())
    request.add_header('Content-Type', IMAGE_SUFFIXES[path.suffix.lower()])
    return play._send(request)


def main():
    for stream in (sys.stdout, sys.stderr):
        stream.reconfigure(encoding='utf-8')
    parser = argparse.ArgumentParser(description='Upload store listing texts and images from fastlane/metadata/android via the Play Developer API.')
    parser.add_argument('command', choices=['plan', 'apply'])
    parser.add_argument('--package', default='com.pavko.hauntscope')
    parser.add_argument('--key', default=os.environ.get('PLAY_JSON_KEY'), help='service account JSON (default: env PLAY_JSON_KEY)')
    parser.add_argument('--langs', help='comma-separated subset of languages')
    parser.add_argument('--texts-only', action='store_true', help='leave the images in Play as they are')
    parser.add_argument('--yes', action='store_true', help='apply: commit the edit to Google Play')
    args = parser.parse_args()

    langs = sorted(p.name for p in META.iterdir() if p.is_dir())
    if args.langs:
        langs = [l for l in langs if l in args.langs.split(',')]
    local = {lang: local_listing(lang) for lang in langs}
    problems = []
    for lang, (listing, images) in local.items():
        for field, limit in LIMITS.items():
            if not 0 < len(listing[field]) <= limit:
                problems.append('%s %s length %d/%d' % (lang, field, len(listing[field]), limit))
        if not args.texts_only and not 2 <= len(images.get('phoneScreenshots', [])) <= 8:
            problems.append('%s needs 2-8 phone screenshots' % lang)
    if problems:
        print('PROBLEMS:\n - ' + '\n - '.join(problems))
        return 1
    if not args.key or not Path(args.key).exists():
        print('service account key not found: pass --key or set PLAY_JSON_KEY')
        return 2

    play = Play(args.key, args.package)
    edit = None
    try:
        edit = play.call('POST', '/edits', body={})['id']
        details = play.call('GET', '/edits/%s/details' % edit)
        remote = {l['language']: l for l in play.call('GET', '/edits/%s/listings' % edit).get('listings', [])}
        print('default language in Play: %s; listings in Play: %s' % (details.get('defaultLanguage'), ', '.join(sorted(remote)) or 'none'))
        work = []
        for lang in langs:
            listing, images = local[lang]
            current = remote.get(lang, {})
            changed = [f for f in LIMITS if current.get(f, '') != listing[f]]
            image_work = []
            for kind, _ in [] if args.texts_only else IMAGE_TYPES:
                files = images.get(kind, [])
                existing = play.call('GET', '/edits/%s/listings/%s/%s' % (edit, lang, kind)).get('images', []) if lang in remote else []
                if [i.get('sha256') for i in existing] != [sha256(f) for f in files]:
                    image_work.append((kind, files, len(existing)))
            status = 'NEW' if lang not in remote else ('UPDATE' if changed or image_work else 'SAME')
            parts = []
            if changed:
                parts.append('texts: ' + ', '.join(changed))
            for kind, files, old in image_work:
                parts.append('%s %d -> %d' % (kind, old, len(files)))
            print('%-6s %-6s %s  | %s' % (lang, status, '; '.join(parts), listing['title']))
            work.append((lang, listing, changed or lang not in remote, image_work))
        if args.command == 'plan' or not args.yes:
            print('\ndry run: nothing committed.' + ('' if args.command == 'plan' else ' Add --yes to publish.'))
            return 0
        for lang, listing, texts, image_work in work:
            if texts:
                play.call('PUT', '/edits/%s/listings/%s' % (edit, lang), body=listing)
            for kind, files, old in image_work:
                if old:
                    play.call('DELETE', '/edits/%s/listings/%s/%s' % (edit, lang, kind))
                for path in files:
                    upload(play, edit, lang, kind, path)
            print('staged %s' % lang)
        try:
            play.call('POST', '/edits/%s:commit' % edit)
        except PlayError as error:
            if 'changesNotSentForReview' not in str(error):
                raise
            play.call('POST', '/edits/%s:commit' % edit, query={'changesNotSentForReview': 'true'})
            print('committed without sending for review (send changes for review in Play Console)')
        edit = None
        print('committed')
        return 0
    except PlayError as error:
        print('\nERROR: %s' % error)
        return 3
    finally:
        if edit:
            try:
                play.call('DELETE', '/edits/%s' % edit)
            except PlayError:
                pass


if __name__ == '__main__':
    sys.exit(main())
