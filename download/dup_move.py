# -*- coding: utf-8 -*-

import csv
import os

FILENAME = './info_dd.csv'

def main():
    print('Move duplicate pages')

    with open(FILENAME) as f_input:
        reader = csv.reader(f_input, delimiter=';', quotechar='"')

        num_duplicates = 0

        for i, row in enumerate(reader):
            lineno = i + 1
            page_id = int(row[2])

            if lineno != page_id:
                file_name = f'./pages/{lineno}.txt'
                if os.path.isfile(file_name):
                    os.rename(file_name, f'./pages/duplicates/{lineno}.txt')
                    num_duplicates += 1

    print(f'Number of duplicate pages: {num_duplicates}')

if __name__ == '__main__':
    main()
