# -*- coding: utf-8 -*-
import urllib.request
import bs4
import time
import sys

BASE_PAGE = 'https://lurkmore.wtf'
CATEGORIES_PAGE = f'{BASE_PAGE}/index.php?title=%D0%A1%D0%BB%D1%83%D0%B6%D0' + \
                   '%B5%D0%B1%D0%BD%D0%B0%D1%8F:%D0%9A%D0%B0%D1%82%D0%B5%D0%B3%D0%BE' + \
                   '%D1%80%D0%B8%D0%B8&offset=&limit=500'

PAGE_EDIT_URL = f'{BASE_PAGE}/index.php?title={{0}}&action=edit'

NEXT_LINK_TITLE = 'следующая страница'

DELAY = 0.3

def main(skip_to_id):
    if skip_to_id > 0:
        print(f'Skip to ID: {skip_to_id}')

    print('Downloading links...')
    categories_and_pages, total = download_categories_and_pages()

    print(f'Total page links downloaded: {total}')
    save_titles(categories_and_pages)

    print('Downloading pages...')
    download_and_dump_pages(categories_and_pages, total, skip_to_id)

def download_categories_and_pages():
    categories_dom = download_and_parse(CATEGORIES_PAGE)
    category_links = get_active_links_from_page(categories_dom)

    categories_and_pages = [
        (item.attrs['href'], item.text, list()) for item in category_links
    ]

    pagelinks_count = 0

    for cat_info in categories_and_pages:
        cat_rel_link, cat_title, pages = cat_info
        print(cat_title)
        pages.extend(fetch_category_pages(cat_rel_link))
        pagelinks_count += len(pages)

    return categories_and_pages, pagelinks_count
        
def fetch_category_pages(category_rel_link):
    category_url = f'{BASE_PAGE}{category_rel_link}'

    all_page_links = []

    while category_url is not None:
        category_dom = download_and_parse(category_url)
        page_links = list(get_active_links_from_page(category_dom))
        category_url = None

        for dom_link in page_links:
            if dom_link.text.lower() == NEXT_LINK_TITLE:
                category_url = f'{BASE_PAGE}{dom_link.attrs["href"]}'
            else:
                all_page_links.append(dom_link)

        time.sleep(DELAY)

    return [(item.attrs['href'], item.text) for item in all_page_links]

def download_and_parse(url):
    with urllib.request.urlopen(url, timeout=10) as resp:
        return bs4.BeautifulSoup(resp, 'html.parser')

def get_active_links_from_page(page_dom):
    links_block = page_dom.find('div', class_='mw-spcontent')

    if links_block is None:
        links_block = page_dom.find('div', class_='mw-content-ltr')

    return filter(lambda e: e.attrs['href'].startswith('/'), links_block.find_all('a')) \
                if links_block is not None \
                else []

def save_titles(infos):
    with open('info.csv', 'w') as f_info:
        lines = []

        for _, cat_title, page_infos in infos:
            for _, page_title in page_infos:
                lines.append(f'"{cat_title}";"{page_title}"\n')
        
        f_info.writelines(lines)

def download_and_dump_pages(infos, total_count, skip):
    page_id = 1

    for _, cat_title, page_infos in infos:
        for page_url, page_title in page_infos:
            print(f'{page_id}/{total_count}: {cat_title} -> {page_title}')
            
            if len(cat_title) <= 0:
                print('empty')
                continue

            item_edit_url = PAGE_EDIT_URL.format(page_url[1:])
            
            if page_id > skip:
                try:
                    download_and_save_page(item_edit_url, page_id)
                    time.sleep(DELAY)
                except Exception as e:
                    print(e)

            page_id += 1

def download_and_save_page(url, id):
    with urllib.request.urlopen(url, timeout=10) as resp:
        item_page_dom = bs4.BeautifulSoup(resp, 'html.parser')

    item_textarea = item_page_dom.find('textarea')
    with open(f'./pages/{id}.txt', 'w') as f_page:
        f_page.write(item_textarea.text)

if __name__ == '__main__':
    skip_to_id = int(sys.argv[1]) if len(sys.argv) > 1 else 0
    main(skip_to_id)
