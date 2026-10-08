import argparse
import json
import sys
import urllib.request
from datetime import date
from pathlib import Path

WB = 'https://api.worldbank.org/v2/country/all/indicator/%s?format=json&per_page=30000&mrv=6'
PPP = 'PA.NUS.PRVT.PP'
FX = 'PA.NUS.FCRF'
GDP = 'NY.GDP.PCAP.CD'
MANUAL = {
    'TW': (0.8, 'estimate: no World Bank data; high income, price level below US'),
    'GI': (1.0, 'no data; high-income territory, base price'),
    'LI': (1.0, 'no data; high-income territory, base price'),
    'MC': (1.0, 'no data; high-income territory, base price'),
    'VA': (1.0, 'no data; base price'),
    'VG': (1.0, 'no data; high-income territory, base price'),
}


def series(indicator):
    with urllib.request.urlopen(WB % indicator, timeout=120) as response:
        data = json.load(response)
    if len(data) < 2:
        raise SystemExit('World Bank: %s: %s' % (indicator, data[0]))
    out = {}
    for row in data[1]:
        if row['value'] is not None and len(row['country']['id']) == 2:
            out.setdefault(row['country']['id'], {})[int(row['date'])] = row['value']
    return out


def main():
    for stream in (sys.stdout, sys.stderr):
        stream.reconfigure(encoding='utf-8')
    parser = argparse.ArgumentParser(description='Regional price factors from World Bank price levels and income.')
    parser.add_argument('--out', required=True)
    parser.add_argument('--income-reference', type=float, default=60000, help='GDP per capita (USD) at which the base price applies')
    parser.add_argument('--floor', type=float, default=0.3)
    parser.add_argument('--poor-without-data', default='SO,TM,VE,YE,ER', help='regions without price data that get the floor factor')
    args = parser.parse_args()

    ppp, fx, gdp = series(PPP), series(FX), series(GDP)
    factors = {}
    for code in sorted(set(ppp) | set(gdp)):
        years = sorted(set(ppp.get(code, {})) & set(fx.get(code, {})))
        income = gdp.get(code)
        if not years or not income:
            continue
        year = years[-1]
        pli = ppp[code][year] / fx[code][year]
        income_year = max(income)
        weight = min(1.0, income[income_year] / args.income_reference)
        factor = min(1.0, max(args.floor, pli + (1 - pli) * weight))
        factors[code] = {'factor': round(factor, 3), 'pli': round(pli, 3), 'pli_year': year, 'gdp_per_capita': round(income[income_year]), 'gdp_year': income_year, 'source': 'worldbank'}
    for code in filter(None, args.poor_without_data.split(',')):
        if code not in factors:
            factors[code] = {'factor': args.floor, 'source': 'no price data; low income, floor'}
    for code, (factor, note) in MANUAL.items():
        if code not in factors:
            factors[code] = {'factor': factor, 'source': note}
    result = {
        'generated': date.today().isoformat(),
        'formula': 'factor = clamp(PLI + (1 - PLI) * min(1, GDPpc / %g), %g, 1); PLI = PPP private consumption (%s) / exchange rate (%s), same year' % (args.income_reference, args.floor, PPP, FX),
        'sources': ['https://data.worldbank.org/indicator/' + PPP, 'https://data.worldbank.org/indicator/' + FX, 'https://data.worldbank.org/indicator/' + GDP],
        'factors': factors,
    }
    Path(args.out).write_text(json.dumps(result, ensure_ascii=False, indent=1) + '\n', encoding='utf-8')
    print('%d regions written to %s' % (len(factors), args.out))
    return 0


if __name__ == '__main__':
    sys.exit(main())
