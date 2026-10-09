import argparse
import csv
import io
import os
import sys
from pathlib import Path

HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE.parent / 'PlayIap'))
from play_iap import Play, PlayError

TEMPLATE = HERE / 'template.csv'
OUTPUT = HERE / 'hauntscope_data_safety.csv'

# Hauntscope's answers (GDD 5.39, privacy policy of 2026-10-09). Sources of each data type:
# AdMob + UMP collect and share IP-derived location, ad interactions, diagnostics and the advertising ID for
# advertising, analytics and fraud prevention. Firebase Analytics (advertising ID off) collects the app instance ID,
# IP-derived location, app interactions and Play purchase events for analytics; Firebase Cloud Messaging collects the
# installation ID after the player turns notifications on. ARCore collects device IDs and diagnostics. Play Billing's
# payment data is never seen by the app. Photos leave the device only through the share sheet the player opens.
GENERAL = {
    'PSL_DATA_COLLECTION_COLLECTS_PERSONAL_DATA': 'true',
    'PSL_DATA_COLLECTION_ENCRYPTED_IN_TRANSIT': 'true',
    ('PSL_SUPPORTED_ACCOUNT_CREATION_METHODS', 'PSL_ACM_NONE'): 'true',
    ('PSL_SUPPORT_DATA_DELETION_BY_USER', 'DATA_DELETION_NO'): 'true',
    'PSL_HAS_OUTSIDE_APP_ACCOUNTS': 'false',
}

ADS = ['PSL_ANALYTICS', 'PSL_FRAUD_PREVENTION_SECURITY', 'PSL_ADVERTISING']

# data type -> (category, collected purposes, shared purposes); every type here is required and not ephemeral.
DATA_TYPES = {
    'PSL_APPROX_LOCATION': ('LOCATION', ADS, ADS),
    'PSL_USER_INTERACTION': ('APP_ACTIVITY', ADS, ADS),
    'PSL_PERFORMANCE_DIAGNOSTICS': ('APP_PERFORMANCE', ADS, ADS),
    'PSL_DEVICE_ID': ('IDENTIFIERS', ['PSL_APP_FUNCTIONALITY', 'PSL_DEVELOPER_COMMUNICATIONS'] + ADS, ADS),
    'PSL_PURCHASE_HISTORY': ('FINANCIAL', ['PSL_ANALYTICS'], []),
}


def answers():
    values = {}
    for key, value in GENERAL.items():
        values[key if isinstance(key, tuple) else (key, '')] = value
    for data_type, (category, collected, shared) in DATA_TYPES.items():
        prefix = 'PSL_DATA_USAGE_RESPONSES:%s:' % data_type
        values[('PSL_DATA_TYPES_' + category, data_type)] = 'true'
        values[(prefix + 'PSL_DATA_USAGE_COLLECTION_AND_SHARING', 'PSL_DATA_USAGE_ONLY_COLLECTED')] = 'true'
        if shared:
            values[(prefix + 'PSL_DATA_USAGE_COLLECTION_AND_SHARING', 'PSL_DATA_USAGE_ONLY_SHARED')] = 'true'
        values[(prefix + 'PSL_DATA_USAGE_EPHEMERAL', '')] = 'false'
        values[(prefix + 'DATA_USAGE_USER_CONTROL', 'PSL_DATA_USAGE_USER_CONTROL_REQUIRED')] = 'true'
        for purpose in collected:
            values[(prefix + 'DATA_USAGE_COLLECTION_PURPOSE', purpose)] = 'true'
        for purpose in shared:
            values[(prefix + 'DATA_USAGE_SHARING_PURPOSE', purpose)] = 'true'
    return values


def build():
    rows = list(csv.reader(TEMPLATE.open(encoding='utf-8', newline='')))
    header, body = rows[0], rows[1:]
    values = answers()
    known = {(row[0], row[1]) for row in body}
    missing = [key for key in values if key not in known]
    if missing:
        raise SystemExit('not in the template: %s' % missing)
    for row in body:
        row[2] = values.get((row[0], row[1]), '')
    text = io.StringIO()
    writer = csv.writer(text, lineterminator='\n')
    writer.writerow(header)
    writer.writerows(body)
    return text.getvalue(), sum(1 for row in body if row[2])


def main():
    for stream in (sys.stdout, sys.stderr):
        stream.reconfigure(encoding='utf-8')
    parser = argparse.ArgumentParser(description='Fill the Play Data safety form from the answers in this script and upload it.')
    parser.add_argument('command', choices=['build', 'apply'])
    parser.add_argument('--package', default='com.pavko.hauntscope')
    parser.add_argument('--key', default=os.environ.get('PLAY_JSON_KEY'), help='service account JSON (default: env PLAY_JSON_KEY)')
    parser.add_argument('--yes', action='store_true', help='apply: actually replace the form in Google Play')
    args = parser.parse_args()

    labels, answered = build()
    OUTPUT.write_text(labels, encoding='utf-8', newline='\n')
    print('%s: %d answered rows, %d data types' % (OUTPUT.name, answered, len(DATA_TYPES)))
    if args.command == 'build':
        return 0
    if not args.yes:
        print('dry run: add --yes to replace the Data safety form in Google Play.')
        return 0
    if not args.key or not Path(args.key).exists():
        print('service account key not found: pass --key or set PLAY_JSON_KEY')
        return 2
    try:
        Play(args.key, args.package).call('POST', '/dataSafety', body={'safetyLabels': labels})
    except PlayError as error:
        print('ERROR: %s' % error)
        return 3
    print('Data safety form replaced in Google Play.')
    return 0


if __name__ == '__main__':
    sys.exit(main())
