// Mythril PWA Service Worker
const CACHE_NAME = 'mythril-cache-v1';
const ASSETS_TO_CACHE = [
    './',
    'index.html',
    'manifest.json',
    'favicon.png',
    'icon-192.png',
    'icon-512.png',
    'css/app.css',
    'css/dark-theme.css',
    'css/light-theme.css',
    'css/completion-effects.css',
    'data/content_graph.json',
    'data/stat_augments.json'
];

self.addEventListener('install', (event) => {
    self.skipWaiting();
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => {
            return cache.addAll(ASSETS_TO_CACHE).catch(err => {
                console.warn('[SW] Caching non-critical asset failed:', err);
            });
        })
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys().then((keys) => {
            return Promise.all(
                keys.map((key) => {
                    if (key !== CACHE_NAME) {
                        return caches.delete(key);
                    }
                })
            );
        }).then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', (event) => {
    if (event.request.method !== 'GET') return;

    event.respondWith(
        caches.match(event.request).then((cachedResponse) => {
            if (cachedResponse) {
                fetch(event.request).then((networkResponse) => {
                    if (networkResponse && networkResponse.status === 200) {
                        caches.open(CACHE_NAME).then((cache) => {
                            cache.put(event.request, networkResponse);
                        });
                    }
                }).catch(() => {});
                return cachedResponse;
            }
            return fetch(event.request).catch(() => {
                if (event.request.headers.get('accept')?.includes('text/html')) {
                    return caches.match('index.html');
                }
            });
        })
    );
});
