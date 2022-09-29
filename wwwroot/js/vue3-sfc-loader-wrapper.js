const options = {
    moduleCache: {
        vue: Vue
    },
    async getFile(url) {
        const res = await fetch(url);
        if (!res.ok)
            throw Object.assign(new Error(res.statusText + ' ' + url), { res });
        return {
            getContentData: asBinary => asBinary ? res.arrayBuffer() : res.text(),
        }
    },
    addStyle(textContent) {
        const style = Object.assign(document.createElement('style'), { textContent });
        const ref = document.head.getElementsByTagName('style')[0] || null;
        document.head.insertBefore(style, ref);
    },
}
var httpVueLoader = function (componentPath) {
    const sfcLoaderOptions = {
        moduleCache: {
            vue: Vue
        },

        getFile: async function (url) {
            const response = await fetch(url);
            if (!response.ok) {
                throw Object.assign(new Error(response.statusText + ' ' + url), { response });
            }
            return await response.text();
        },

        addStyle: function (textContent) {
            const style = Object.assign(document.createElement('style'), { textContent });
            const reference = document.head.getElementsByTagName('style')[0] || null;
            document.head.insertBefore(style, reference);
        }
    };

    return Vue.defineAsyncComponent(function () {
        return window['vue3-sfc-loader'].loadModule(componentPath, options);
        //return window['vue3-sfc-loader'].loadModule(componentPath, sfcLoaderOptions);
    });
}