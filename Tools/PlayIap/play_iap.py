import argparse
import base64
import csv
import json
import math
import os
import re
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from decimal import Decimal, InvalidOperation
from pathlib import Path

API = 'https://androidpublisher.googleapis.com/androidpublisher/v3/applications/'
SCOPE = 'https://www.googleapis.com/auth/androidpublisher'
TITLE_MAX = 55
DESCRIPTION_MAX = 200
PRODUCT_ID = re.compile(r'^[a-z0-9][a-z0-9_.]*$')
OPTION_ID = re.compile(r'^[a-z0-9][a-z0-9-]{0,62}$')
GRID_USD = ['0.29', '0.39', '0.49', '0.59', '0.69', '0.79', '0.89', '0.99', '1.09', '1.19', '1.29', '1.39', '1.49', '1.59', '1.69', '1.79', '1.89', '1.99', '2.19', '2.39', '2.49', '2.69', '2.79', '2.99', '3.29', '3.49', '3.79', '3.99', '4.29', '4.49', '4.79', '4.99', '5.49', '5.99', '6.49', '6.99', '7.49', '7.99', '8.49', '8.99', '9.49', '9.99', '10.99', '11.99', '12.99', '14.99', '17.99', '19.99', '24.99', '29.99', '39.99', '49.99', '99.99']
LATENCY = {'sensitive': 'PRODUCT_UPDATE_LATENCY_TOLERANCE_LATENCY_SENSITIVE', 'tolerant': 'PRODUCT_UPDATE_LATENCY_TOLERANCE_LATENCY_TOLERANT'}


class PlayError(Exception):
    pass


def load_catalog(path):
    path = Path(path).resolve()
    catalog = json.loads(path.read_text(encoding='utf-8'))
    catalog['_dir'] = path.parent
    catalog['_factors'] = {}
    if catalog.get('regional_factors'):
        data = json.loads((path.parent / catalog['regional_factors']).read_text(encoding='utf-8'))
        catalog['_factors'] = data['factors']
    return catalog


def money_value(value):
    return Decimal(value.get('units', '0')) + Decimal(value.get('nanos', 0)) / Decimal(1000000000)


def parse_price(value):
    try:
        price = Decimal(str(value))
    except InvalidOperation:
        return None
    if price <= 0 or price.as_tuple().exponent < -2:
        return None
    return price


def money(currency, amount):
    amount = Decimal(str(amount))
    units = int(amount)
    nanos = int(((amount - units) * Decimal(1000000000)).to_integral_value())
    return {'currencyCode': currency, 'units': str(units), 'nanos': nanos}


def money_text(value):
    if not value:
        return '-'
    amount = money_value(value)
    text = str(int(amount)) if amount == amount.to_integral_value() else str(amount.quantize(Decimal('0.01')))
    return '%s %s' % (text, value.get('currencyCode', ''))


def check_game_texts(catalog, product):
    # Optional in-game texts ("game": {locale: {name, description}}) are never sent to Play; checked only when game_languages is set.
    languages = catalog.get('game_languages')
    if not languages:
        return []
    problems = []
    pid = product.get('id', '')
    texts = product.get('game', {})
    limits = {'name': catalog.get('game_name_max', 24), 'description': catalog.get('game_description_max', 90)}
    banned = re.compile(catalog['banned'], re.I) if catalog.get('banned') else None
    for lang in languages:
        if lang not in texts:
            problems.append('%s: no game texts for %s' % (pid, lang))
    for lang, entry in texts.items():
        if lang not in languages:
            problems.append('%s: game texts %s are not in game_languages' % (pid, lang))
        for field, limit in limits.items():
            if not 0 < len(entry.get(field, '')) <= limit:
                problems.append('%s %s: game %s length %d (1-%d)' % (pid, lang, field, len(entry.get(field, '')), limit))
            found = banned.search(entry.get(field, '')) if banned else None
            if found:
                problems.append('%s %s: banned term %r in game %s' % (pid, lang, found.group(), field))
    return problems


def check(catalog, warn=print):
    problems = []
    languages = catalog.get('languages') or []
    if not catalog.get('package'):
        problems.append('package is missing')
    if catalog.get('default_language') not in languages:
        problems.append('default_language must be one of languages')
    if len(set(languages)) != len(languages):
        problems.append('languages contain duplicates')
    if not OPTION_ID.match(catalog.get('purchase_option_id', '')):
        problems.append('purchase_option_id must be 1-63 chars: lowercase letters, digits, hyphens')
    banned = re.compile(catalog['banned'], re.I) if catalog.get('banned') else None
    seen = set()
    for product in catalog.get('products', []):
        pid = product.get('id', '')
        if not PRODUCT_ID.match(pid):
            problems.append('%s: bad product id' % pid)
        if pid in seen:
            problems.append('%s: duplicate id' % pid)
        seen.add(pid)
        if parse_price(product.get('price_usd')) is None:
            problems.append('%s: price_usd must be a positive number with up to 2 decimals' % pid)
        listings = product.get('listings', {})
        for lang in languages:
            if lang not in listings:
                problems.append('%s: no listing for %s' % (pid, lang))
        for lang, listing in listings.items():
            if lang not in languages:
                problems.append('%s: listing %s is not in languages' % (pid, lang))
            title = listing.get('title', '')
            description = listing.get('description', '')
            if not 0 < len(title) <= TITLE_MAX:
                problems.append('%s %s: title length %d (1-%d)' % (pid, lang, len(title), TITLE_MAX))
            if not 0 < len(description) <= DESCRIPTION_MAX:
                problems.append('%s %s: description length %d (1-%d)' % (pid, lang, len(description), DESCRIPTION_MAX))
            for text in (title, description):
                found = banned.search(text) if banned else None
                if found:
                    problems.append('%s %s: banned term %r' % (pid, lang, found.group()))
        for region, value in product.get('region_prices', {}).items():
            if parse_price(value) is None:
                problems.append('%s: bad region price for %s' % (pid, region))
        problems += check_game_texts(catalog, product)
    source = catalog.get('expect_ids_in')
    if source:
        source_path = (catalog['_dir'] / source).resolve()
        if not source_path.exists():
            warn('WARNING: expect_ids_in not found, ids not checked: %s' % source_path)
        else:
            text = source_path.read_text(encoding='utf-8')
            for pid in seen:
                if '"%s"' % pid not in text:
                    problems.append('%s: id not found in %s' % (pid, source_path.name))
    return problems


def print_catalog(catalog):
    default = catalog['default_language']
    print('package %s, %d products, %d languages, option id "%s", %d regional factors' % (catalog['package'], len(catalog['products']), len(catalog['languages']), catalog['purchase_option_id'], len(catalog['_factors'])))
    print('%-24s %8s  %5s %5s  %s' % ('id', 'usd', 'title', 'desc', default + ' title'))
    for product in catalog['products']:
        listings = product['listings'].values()
        print('%-24s %8s  %5d %5d  %s' % (product['id'], product['price_usd'], max(len(x['title']) for x in listings), max(len(x['description']) for x in listings), product['listings'][default]['title']))


class Play:
    def __init__(self, key_path, package):
        self.package = package
        self.key = json.loads(Path(key_path).read_text(encoding='utf-8'))
        self.token = None

    def _sign(self, data):
        try:
            from cryptography.hazmat.primitives import hashes, serialization
            from cryptography.hazmat.primitives.asymmetric import padding
        except ImportError:
            raise PlayError('python package "cryptography" is required: pip install cryptography')
        private = serialization.load_pem_private_key(self.key['private_key'].encode('utf-8'), password=None)
        return private.sign(data, padding.PKCS1v15(), hashes.SHA256())

    def _auth(self):
        if self.token and self.token[1] > time.time() + 60:
            return self.token[0]
        b64 = lambda raw: base64.urlsafe_b64encode(raw).rstrip(b'=')
        now = int(time.time())
        token_uri = self.key.get('token_uri', 'https://oauth2.googleapis.com/token')
        header = b64(json.dumps({'alg': 'RS256', 'typ': 'JWT'}).encode())
        claims = b64(json.dumps({'iss': self.key['client_email'], 'scope': SCOPE, 'aud': token_uri, 'iat': now, 'exp': now + 3600}).encode())
        unsigned = header + b'.' + claims
        assertion = unsigned + b'.' + b64(self._sign(unsigned))
        body = urllib.parse.urlencode({'grant_type': 'urn:ietf:params:oauth:grant-type:jwt-bearer', 'assertion': assertion.decode()}).encode()
        reply = self._send(urllib.request.Request(token_uri, data=body, method='POST'))
        self.token = (reply['access_token'], now + int(reply.get('expires_in', 3600)))
        return self.token[0]

    def _send(self, request):
        try:
            with urllib.request.urlopen(request, timeout=60) as response:
                raw = response.read()
        except urllib.error.HTTPError as error:
            detail = error.read().decode('utf-8', 'replace')
            try:
                detail = json.loads(detail)['error']['message']
            except (ValueError, KeyError, TypeError):
                pass
            raise PlayError('%s %s: HTTP %d: %s' % (request.get_method(), request.full_url.split('?')[0], error.code, detail))
        return json.loads(raw) if raw else {}

    def call(self, method, path, query=None, body=None):
        url = API + urllib.parse.quote(self.package) + path
        if query:
            url += '?' + urllib.parse.urlencode(query, doseq=True)
        data = json.dumps(body).encode('utf-8') if body is not None else None
        request = urllib.request.Request(url, data=data, method=method)
        request.add_header('Authorization', 'Bearer ' + self._auth())
        if data is not None:
            request.add_header('Content-Type', 'application/json; charset=utf-8')
        return self._send(request)

    def products(self):
        found, token = [], None
        while True:
            query = {'pageSize': 1000}
            if token:
                query['pageToken'] = token
            reply = self.call('GET', '/oneTimeProducts', query)
            found += reply.get('oneTimeProducts', [])
            token = reply.get('nextPageToken')
            if not token:
                return {p['productId']: p for p in found}

    def convert(self, usd):
        key = str(Decimal(str(usd)))
        cache = self.__dict__.setdefault('_converted', {})
        if key not in cache:
            cache[key] = self.call('POST', '/pricing:convertRegionPrices', body={'price': money('USD', key)})
        return cache[key]


def option_of(product, option_id):
    for option in product.get('purchaseOptions', []):
        if option.get('purchaseOptionId') == option_id:
            return option
    return None


def listings_of(catalog, product):
    return [{'languageCode': lang, 'title': product['listings'][lang]['title'], 'description': product['listings'][lang]['description']} for lang in catalog['languages']]


def grid_point(base, factor, grid):
    target = base * Decimal(str(factor))
    candidates = [g for g in (Decimal(x) for x in grid) if g <= base] or [base]
    return min(candidates, key=lambda g: abs(math.log(float(g) / float(target))))


def regional_price(catalog, play, product, region, converted):
    base = parse_price(product['price_usd'])
    factor = catalog['_factors'].get(region, {}).get('factor', 1.0)
    if factor >= 0.995:
        return converted['convertedRegionPrices'][region]['price']
    point = grid_point(base, factor, catalog.get('price_grid_usd', GRID_USD))
    scaled = play.convert(point)['convertedRegionPrices'].get(region)
    return scaled['price'] if scaled else converted['convertedRegionPrices'][region]['price']


def build(catalog, play, product, converted):
    overrides = product.get('region_prices', {})
    regional = []
    for region in sorted(converted['convertedRegionPrices']):
        price = regional_price(catalog, play, product, region, converted)
        if region in overrides:
            price = money(price['currencyCode'], overrides[region])
        regional.append({'regionCode': region, 'price': price, 'availability': 'AVAILABLE'})
    other = converted['convertedOtherRegionsPrice']
    return {
        'packageName': catalog['package'],
        'productId': product['id'],
        'listings': listings_of(catalog, product),
        'purchaseOptions': [{
            'purchaseOptionId': catalog['purchase_option_id'],
            'buyOption': {'legacyCompatible': True, 'multiQuantityEnabled': False},
            'regionalPricingAndAvailabilityConfigs': regional,
            'newRegionsConfig': {'usdPrice': other['usdPrice'], 'eurPrice': other['eurPrice'], 'availability': 'AVAILABLE'},
        }],
    }


def listing_key(listings):
    return sorted((x['languageCode'], x['title'], x['description']) for x in listings)


def make_plan(catalog, play, regions_version=None):
    existing = play.products()
    plan = {'regionsVersion': regions_version, 'create': [], 'update': [], 'same': [], 'activate': [], 'extra': []}
    show = catalog.get('show_regions', ['US'])
    option_id = catalog['purchase_option_id']
    for product in catalog['products']:
        pid = product['id']
        current = existing.get(pid)
        if current is None:
            converted = play.convert(product['price_usd'])
            plan['regionsVersion'] = plan['regionsVersion'] or converted['regionVersion']['version']
            body = build(catalog, play, product, converted)
            prices = {r['regionCode']: r['price'] for r in body['purchaseOptions'][0]['regionalPricingAndAvailabilityConfigs']}
            plan['create'].append({'id': pid, 'body': body, 'sample': {r: money_text(prices.get(r)) for r in show}, 'regions': len(prices)})
            continue
        option = option_of(current, option_id)
        notes = []
        if option is None:
            notes.append('no purchase option "%s" in Play (not changed by this script)' % option_id)
        else:
            us = next((r['price'] for r in option.get('regionalPricingAndAvailabilityConfigs', []) if r['regionCode'] == 'US'), None)
            if us and Decimal(money_text(us).split()[0]) != parse_price(product['price_usd']):
                notes.append('US price in Play %s, catalog %s USD (change prices in Play Console)' % (money_text(us), product['price_usd']))
            if option.get('state') != 'ACTIVE':
                plan['activate'].append({'id': pid, 'state': option.get('state')})
        if listing_key(current.get('listings', [])) != listing_key(listings_of(catalog, product)):
            plan['update'].append({'id': pid, 'body': {'packageName': catalog['package'], 'productId': pid, 'listings': listings_of(catalog, product)}, 'notes': notes})
        else:
            plan['same'].append({'id': pid, 'notes': notes})
    catalog_ids = {p['id'] for p in catalog['products']}
    plan['extra'] = sorted(pid for pid in existing if pid not in catalog_ids)
    if not plan['regionsVersion'] and (plan['create'] or plan['update']):
        plan['regionsVersion'] = play.convert(catalog['products'][0]['price_usd'])['regionVersion']['version']
    return plan


def write_report(catalog, play, plan, path):
    created = {item['id']: item for item in plan['create']}
    products = [p for p in catalog['products'] if p['id'] in created]
    if not products:
        print('report: nothing to create, skipped')
        return
    regions = sorted({r['regionCode'] for item in created.values() for r in item['body']['purchaseOptions'][0]['regionalPricingAndAvailabilityConfigs']})
    with open(path, 'w', encoding='utf-8-sig', newline='') as handle:
        writer = csv.writer(handle)
        header = ['region', 'currency', 'factor', 'factor source']
        for product in products:
            header += [product['id'], product['id'] + ' ~USD']
        writer.writerow(header)
        for region in regions:
            info = catalog['_factors'].get(region, {})
            row = [region, '', info.get('factor', 1.0), info.get('source', 'no factor: base price')]
            for product in products:
                configs = {r['regionCode']: r['price'] for r in created[product['id']]['body']['purchaseOptions'][0]['regionalPricingAndAvailabilityConfigs']}
                local = configs.get(region)
                default = play.convert(product['price_usd'])['convertedRegionPrices'].get(region)
                usd = ''
                if local and default and money_value(default['price']):
                    usd = str((money_value(local) / money_value(default['price']) * parse_price(product['price_usd'])).quantize(Decimal('0.01')))
                row[1] = local['currencyCode'] if local else row[1]
                row += [str(money_value(local)) if local else '', usd]
            writer.writerow(row)
    print('report saved to %s (%d regions)' % (path, len(regions)))


def print_plan(plan):
    print('regions version: %s' % plan['regionsVersion'])
    for item in plan['create']:
        print('CREATE   %-24s %d regions; %s' % (item['id'], item['regions'], ', '.join('%s %s' % kv for kv in item['sample'].items())))
    for item in plan['update']:
        print('LISTINGS %-24s texts differ, will be replaced with --update-listings' % item['id'])
        for note in item['notes']:
            print('         note: ' + note)
    for item in plan['same']:
        print('SAME     %s' % item['id'])
        for note in item['notes']:
            print('         note: ' + note)
    for item in plan['activate']:
        print('ACTIVATE %-24s purchase option state %s' % (item['id'], item['state']))
    for pid in plan['extra']:
        print('EXTRA    %-24s in Play but not in catalog (left untouched)' % pid)


def apply(catalog, play, plan, update_listings, activate, latency):
    requests = []
    for item in plan['create']:
        requests.append({'oneTimeProduct': item['body'], 'updateMask': 'listings,purchaseOptions', 'regionsVersion': {'version': plan['regionsVersion']}, 'allowMissing': True, 'latencyTolerance': latency})
    if update_listings:
        for item in plan['update']:
            requests.append({'oneTimeProduct': item['body'], 'updateMask': 'listings', 'regionsVersion': {'version': plan['regionsVersion']}, 'latencyTolerance': latency})
    for start in range(0, len(requests), 100):
        chunk = requests[start:start + 100]
        reply = play.call('POST', '/oneTimeProducts:batchUpdate', body={'requests': chunk})
        for product in reply.get('oneTimeProducts', []):
            print('written  %s' % product['productId'])
    if not activate:
        return
    option_id = catalog['purchase_option_id']
    catalog_ids = {p['id'] for p in catalog['products']}
    pending = []
    for pid, product in play.products().items():
        option = option_of(product, option_id)
        if pid in catalog_ids and option and option.get('state') != 'ACTIVE':
            pending.append({'activatePurchaseOptionRequest': {'packageName': catalog['package'], 'productId': pid, 'purchaseOptionId': option_id, 'latencyTolerance': latency}})
    for start in range(0, len(pending), 100):
        reply = play.call('POST', '/oneTimeProducts/-/purchaseOptions:batchUpdateStates', body={'requests': pending[start:start + 100]})
        for product in reply.get('oneTimeProducts', []):
            option = option_of(product, option_id) or {}
            print('state    %-24s %s' % (product['productId'], option.get('state')))


def main():
    for stream in (sys.stdout, sys.stderr):
        stream.reconfigure(encoding='utf-8')
    parser = argparse.ArgumentParser(description='Create Google Play one-time products (in-app products) from a JSON catalog.')
    parser.add_argument('command', choices=['check', 'list', 'plan', 'apply'])
    parser.add_argument('--catalog', required=True)
    parser.add_argument('--key', default=os.environ.get('PLAY_JSON_KEY'), help='service account JSON (default: env PLAY_JSON_KEY)')
    parser.add_argument('--out', help='plan: save the full plan with request bodies to this JSON file')
    parser.add_argument('--report', help='plan/apply: save a CSV with every region price of products to create')
    parser.add_argument('--regions-version', help='override the regions version returned by convertRegionPrices')
    parser.add_argument('--update-listings', action='store_true', help='apply: also replace titles/descriptions of existing products')
    parser.add_argument('--no-activate', action='store_true', help='apply: leave purchase options in their current state')
    parser.add_argument('--latency', choices=sorted(LATENCY), default='sensitive')
    parser.add_argument('--yes', action='store_true', help='apply: actually write to Google Play')
    args = parser.parse_args()

    catalog = load_catalog(args.catalog)
    problems = check(catalog)
    print_catalog(catalog)
    if problems:
        print('\nPROBLEMS:')
        for problem in problems:
            print(' - ' + problem)
        return 1
    print('catalog ok')
    if args.command == 'check':
        return 0
    if not args.key or not Path(args.key).exists():
        print('service account key not found: pass --key or set PLAY_JSON_KEY')
        return 2
    play = Play(args.key, catalog['package'])
    try:
        if args.command == 'list':
            products = play.products()
            for pid in sorted(products):
                product = products[pid]
                states = ', '.join('%s=%s' % (o.get('purchaseOptionId'), o.get('state')) for o in product.get('purchaseOptions', []))
                print('%-24s %2d listings  %s' % (pid, len(product.get('listings', [])), states))
            print('%d products in Play' % len(products))
            return 0
        plan = make_plan(catalog, play)
        if args.regions_version:
            plan['regionsVersion'] = args.regions_version
        print()
        print_plan(plan)
        if args.report:
            write_report(catalog, play, plan, args.report)
        if args.out:
            Path(args.out).write_text(json.dumps(plan, ensure_ascii=False, indent=2), encoding='utf-8')
            print('plan saved to %s' % args.out)
        if args.command == 'plan':
            return 0
        if not args.yes:
            print('\ndry run: nothing written. Add --yes to write to Google Play.')
            return 0
        apply(catalog, play, plan, args.update_listings, not args.no_activate, LATENCY[args.latency])
        print('done')
        return 0
    except PlayError as error:
        print('\nERROR: %s' % error)
        return 3


if __name__ == '__main__':
    sys.exit(main())
