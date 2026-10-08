import base64, json, sys, tempfile
from decimal import Decimal
from pathlib import Path
HERE = Path(__file__).resolve().parent
sys.path.insert(0, str(HERE))
import play_iap as m
from cryptography.hazmat.primitives.asymmetric import rsa, padding
from cryptography.hazmat.primitives import serialization, hashes

key = rsa.generate_private_key(public_exponent=65537, key_size=2048)
pem = key.private_bytes(serialization.Encoding.PEM, serialization.PrivateFormat.PKCS8, serialization.NoEncryption()).decode()
tmp = Path(tempfile.mkdtemp()) / 'k.json'
tmp.write_text(json.dumps({'client_email': 'x@y.iam.gserviceaccount.com', 'private_key': pem, 'token_uri': 'https://oauth2.googleapis.com/token'}))

catalog = m.load_catalog(sys.argv[1] if len(sys.argv) > 1 else HERE / 'hauntscope_iap.json')
warnings = []
assert m.check(catalog, warnings.append) == []
broken = json.loads(json.dumps(catalog, default=str))
broken['_dir'] = catalog['_dir']
broken['expect_ids_in'] = 'missing/Ids.cs'
broken['products'][0].get('game', {}).pop(catalog.get('game_languages', ['?'])[0], None)
warnings = []
problems = m.check(broken, warnings.append)
assert len(warnings) == 1 and 'expect_ids_in not found' in warnings[0]
assert not catalog.get('game_languages') or any('no game texts' in p for p in problems)
first, second = catalog['products'][0]['id'], catalog['products'][1]['id']
play = m.Play(tmp, catalog['package'])
calls = []
store = {}

def fake_send(request):
    if 'oauth2' in request.full_url:
        form = dict(x.split('=', 1) for x in request.data.decode().split('&'))
        a = form['assertion'].split('.')
        pad = lambda s: s + '=' * (-len(s) % 4)
        key.public_key().verify(base64.urlsafe_b64decode(pad(a[2])), (a[0] + '.' + a[1]).encode(), padding.PKCS1v15(), hashes.SHA256())
        claims = json.loads(base64.urlsafe_b64decode(pad(a[1])))
        assert claims['scope'] == m.SCOPE and claims['iss'].startswith('x@')
        return {'access_token': 'tok', 'expires_in': 3600}
    assert request.get_header('Authorization') == 'Bearer tok'
    url = request.full_url
    body = json.loads(request.data) if request.data else None
    calls.append((request.get_method(), url.split('/applications/')[1], body))
    if url.endswith('pricing:convertRegionPrices'):
        usd = Decimal(body['price']['units']) + Decimal(body['price']['nanos']) / Decimal(10**9)
        return {'convertedRegionPrices': {
                    'US': {'regionCode': 'US', 'price': m.money('USD', usd)},
                    'UA': {'regionCode': 'UA', 'price': m.money('UAH', (usd * 41).quantize(Decimal('1')) - Decimal('0.01'))},
                    'JP': {'regionCode': 'JP', 'price': m.money('JPY', (usd * 150).quantize(Decimal('1')))}},
                'convertedOtherRegionsPrice': {'usdPrice': m.money('USD', usd), 'eurPrice': m.money('EUR', usd)},
                'regionVersion': {'version': '2025/03'}}
    if '/oneTimeProducts?' in url:
        return {'oneTimeProducts': list(store.values())}
    if url.endswith(':batchUpdate'):
        out = []
        for r in body['requests']:
            p = json.loads(json.dumps(r['oneTimeProduct']))
            assert r['regionsVersion'] == {'version': '2025/03'}
            if p['productId'] in store:
                assert r['updateMask'] == 'listings' and 'allowMissing' not in r
                store[p['productId']]['listings'] = p['listings']
            else:
                assert r['allowMissing'] is True
                for o in p['purchaseOptions']:
                    o['state'] = 'DRAFT'
                store[p['productId']] = p
            out.append(store[p['productId']])
        return {'oneTimeProducts': out}
    if url.endswith('purchaseOptions:batchUpdateStates'):
        assert '/oneTimeProducts/-/' in url
        out = []
        for r in body['requests']:
            a = r['activatePurchaseOptionRequest']
            for o in store[a['productId']]['purchaseOptions']:
                if o['purchaseOptionId'] == a['purchaseOptionId']:
                    o['state'] = 'ACTIVE'
            out.append(store[a['productId']])
        return {'oneTimeProducts': out}
    raise AssertionError(url)

play._send = fake_send
plan = m.make_plan(catalog, play)
m.print_plan(plan)
assert len(plan['create']) == len(catalog['products']) and plan['regionsVersion'] == '2025/03'
b = plan['create'][0]['body']
assert b['purchaseOptions'][0]['buyOption']['legacyCompatible'] is True
assert len(b['listings']) == len(catalog['languages'])
assert 'game' not in b and 'type' not in b and not any('game' in item['body'] for item in plan['create'])
m.apply(catalog, play, plan, False, True, m.LATENCY['sensitive'])
assert all(o['state'] == 'ACTIVE' for p in store.values() for o in p['purchaseOptions'])
print('--- second run')
store[first]['listings'][0]['title'] = 'Old'
store[second]['purchaseOptions'][0]['regionalPricingAndAvailabilityConfigs'][-1]['price'] = m.money('USD', '1.49')
store['legacy_thing'] = {'productId': 'legacy_thing', 'listings': [], 'purchaseOptions': []}
plan2 = m.make_plan(catalog, play)
m.print_plan(plan2)
assert [x['id'] for x in plan2['update']] == [first] and not plan2['create'] and plan2['extra'] == ['legacy_thing']
before = len(calls)
m.apply(catalog, play, plan2, True, True, m.LATENCY['sensitive'])
assert store[first]['listings'][0]['title'] == catalog['products'][0]['listings'][catalog['languages'][0]]['title']
print('calls', len(calls), 'ok')
