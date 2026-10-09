import argparse
import base64
import json
import os
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path

# The game keeps every device with notifications on subscribed to "all" and "lang_<game language>" (GDD 5.36), so a
# message goes out once per language, each in its own words.
LANGUAGES = ['en', 'uk', 'es-419', 'pt-BR', 'de', 'id']
TOPIC_PREFIX = 'lang_'
CHANNEL = 'hauntscope_news'
SCOPE = 'https://www.googleapis.com/auth/firebase.messaging'
# Past these lengths the Android notification shade cuts the text.
TITLE_LIMIT = 50
BODY_LIMIT = 150


class PushError(Exception):
    pass


class Fcm:
    def __init__(self, key_path):
        self.key = json.loads(Path(key_path).read_text(encoding='utf-8'))
        self.project = self.key['project_id']
        self.token = None

    def _auth(self):
        if self.token and self.token[1] > time.time() + 60:
            return self.token[0]
        try:
            from cryptography.hazmat.primitives import hashes, serialization
            from cryptography.hazmat.primitives.asymmetric import padding
        except ImportError:
            raise PushError('python package "cryptography" is required: pip install cryptography')
        b64 = lambda raw: base64.urlsafe_b64encode(raw).rstrip(b'=')
        now = int(time.time())
        token_uri = self.key.get('token_uri', 'https://oauth2.googleapis.com/token')
        header = b64(json.dumps({'alg': 'RS256', 'typ': 'JWT'}).encode())
        claims = b64(json.dumps({'iss': self.key['client_email'], 'scope': SCOPE, 'aud': token_uri, 'iat': now, 'exp': now + 3600}).encode())
        unsigned = header + b'.' + claims
        private = serialization.load_pem_private_key(self.key['private_key'].encode('utf-8'), password=None)
        assertion = unsigned + b'.' + b64(private.sign(unsigned, padding.PKCS1v15(), hashes.SHA256()))
        body = urllib.parse.urlencode({'grant_type': 'urn:ietf:params:oauth:grant-type:jwt-bearer', 'assertion': assertion.decode()}).encode()
        reply = self._send(urllib.request.Request(token_uri, data=body, method='POST'))
        self.token = (reply['access_token'], now + int(reply.get('expires_in', 3600)))
        return self.token[0]

    def _send(self, request):
        try:
            with urllib.request.urlopen(request, timeout=60) as response:
                return json.loads(response.read() or b'{}')
        except urllib.error.HTTPError as error:
            detail = error.read().decode('utf-8', 'replace')
            try:
                detail = json.loads(detail)['error']['message']
            except (ValueError, KeyError, TypeError):
                pass
            raise PushError('HTTP %d: %s' % (error.code, detail))

    def send(self, topic, title, body, validate_only):
        message = {'topic': topic, 'notification': {'title': title, 'body': body},
                   'android': {'priority': 'HIGH', 'notification': {'channel_id': CHANNEL}}}
        url = 'https://fcm.googleapis.com/v1/projects/%s/messages:send' % self.project
        data = json.dumps({'validate_only': validate_only, 'message': message}).encode('utf-8')
        request = urllib.request.Request(url, data=data, method='POST')
        request.add_header('Authorization', 'Bearer ' + self._auth())
        request.add_header('Content-Type', 'application/json; charset=utf-8')
        return self._send(request)['name']


def load(path, langs):
    texts = json.loads(Path(path).read_text(encoding='utf-8'))
    problems, warnings = [], []
    for lang in langs:
        entry = texts.get(lang)
        if not entry or not entry.get('title', '').strip() or not entry.get('body', '').strip():
            problems.append('%s: title and body are required' % lang)
            continue
        if len(entry['title']) > TITLE_LIMIT:
            warnings.append('%s: title %d/%d characters, the shade may cut it' % (lang, len(entry['title']), TITLE_LIMIT))
        if len(entry['body']) > BODY_LIMIT:
            warnings.append('%s: body %d/%d characters, the shade may cut it' % (lang, len(entry['body']), BODY_LIMIT))
    unknown = sorted(set(texts) - set(LANGUAGES))
    if unknown:
        problems.append('unknown languages: %s (game languages: %s)' % (', '.join(unknown), ', '.join(LANGUAGES)))
    return texts, problems, warnings


def main():
    for stream in (sys.stdout, sys.stderr):
        stream.reconfigure(encoding='utf-8')
    parser = argparse.ArgumentParser(description='Send a push to Hauntscope players, one message per game language (FCM HTTP v1).')
    parser.add_argument('command', choices=['check', 'send'])
    parser.add_argument('message', help='JSON file: {"en": {"title": ..., "body": ...}, "uk": {...}, ...}')
    parser.add_argument('--langs', help='comma-separated subset, e.g. "uk" for a test on your own phone')
    parser.add_argument('--key', default=os.environ.get('FIREBASE_JSON_KEY'), help='Firebase admin service account JSON (default: env FIREBASE_JSON_KEY)')
    parser.add_argument('--yes', action='store_true', help='send: really deliver (without it Firebase only validates)')
    args = parser.parse_args()

    langs = LANGUAGES if not args.langs else [l.strip() for l in args.langs.split(',')]
    texts, problems, warnings = load(args.message, langs)
    for lang in langs:
        if lang in texts:
            print('%-6s %s%s | %s' % (lang, TOPIC_PREFIX, lang, texts[lang].get('title', '')))
    for warning in warnings:
        print('WARNING: ' + warning)
    if problems:
        print('PROBLEMS:\n - ' + '\n - '.join(problems))
        return 1
    if args.command == 'check':
        return 0
    if not args.key or not Path(args.key).exists():
        print('Firebase key not found: pass --key or set FIREBASE_JSON_KEY')
        return 2

    fcm = Fcm(args.key)
    try:
        for lang in langs:
            name = fcm.send(TOPIC_PREFIX + lang, texts[lang]['title'], texts[lang]['body'], validate_only=not args.yes)
            print('%s %s -> %s' % ('sent     ' if args.yes else 'validated', lang, name))
    except PushError as error:
        print('ERROR: %s' % error)
        return 3
    if not args.yes:
        print('\nvalidated only: add --yes to deliver.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
