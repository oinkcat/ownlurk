# -*- coding: utf-8 -*-
import csv

INFO_FILENAME = './info.csv'
RESULT_FILENAME = './info_dd.csv'

def main():
    with open(INFO_FILENAME) as f_input:
        reader = csv.reader(f_input, delimiter=';', quotechar='"')
        title_ids = get_title_ids(reader)

        with open(RESULT_FILENAME, 'w') as f_output:
            f_input.seek(0)
            reader = csv.reader(f_input, delimiter=';', quotechar='"')
            writer = csv.writer(f_output, delimiter=';', quotechar='"')

            write_dedup_info(reader, title_ids, writer)

def get_title_ids(info_csv_reader):
    num_total = 0
    titles = dict()

    print('Processing info...')

    for i, row in enumerate(info_csv_reader):
        title = row[1]

        if title not in titles:
            titles[title] = i + 1

        num_total += 1

    print(f'Total items: {num_total}, unique items: {len(titles)}')

    return titles

def write_dedup_info(reader, title_ids, writer):

    print('Writing...')

    for row in reader:
        title = row[1]
        title_id = title_ids[title]

        row.append(title_id)
        writer.writerow(row)

    print('Done')

if __name__ == '__main__':
    main()