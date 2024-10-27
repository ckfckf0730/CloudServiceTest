
class IndexedDB {

    static getDB(dbNamme) {
        const request = indexedDB.open(dbNamme, 1);

        request.onupgradeneeded = function (event) {
            const db = event.target.result;

            // create a file saving table，if not exist
            if (!db.objectStoreNames.contains('files')) {
                db.createObjectStore('files', { keyPath: 'id' }); // use `id` as Primary-key
            }
        };

        request.onsuccess = function (event) {
            const db = event.target.result;
            console.log('indexedDB open/create successfully.');
            return db;
        };

        request.onerror = function (event) {
            console.error('indexedDB open/create failed: ', event);
        };

        return null;
    }


    static addData(db, data) {
        const transaction = db.transaction(['images'], 'readwrite');
        const store = transaction.objectStore('images');

        const request = store.add(data);

        request.onsuccess = function () {
            console.log('Add indexedDB data success.');
        };

        request.onerror = function () {
            console.error('Add indexedDB data failed:', request.error);
        };
    }


    static getData(db, id) {
        const transaction = db.transaction(['images'], 'readonly');
        const store = transaction.objectStore('images');

        const request = store.get(id);

        request.onsuccess = function () {
            const result = request.result;
            if (result) {
                console.log('Get indexedDB data success', result);
                return result;
            } else {
                console.log('Get indexedDB data failed;');
            }
        };

        request.onerror = function () {
            console.error('Get indexedDB data failed;', request.error);
        };
        return null;
    }
}








