// var elements = [];

// function searchShadowRoot(node, id) {
//     if (node == null) {
//         return;
//     }

//     if (node.shadowRoot != undefined && node.shadowRoot != null) {
//         if (!elements.includes(node.shadowRoot)) {
//             elements.push(node.shadowRoot);
//         }
//         searchShadowRoot(node.shadowRoot, id);
//     }

//     for (var i = 0; i < node.childNodes.length; i++) {
//         searchShadowRoot(node.childNodes[i], id);
//     }
// }

// elements.push(document);
// searchShadowRoot(document, 'input');
// searchShadowRoot(document, 'textarea');

// function focusin (e) {
//     const target = e.target;
//     if (target.tagName == 'INPUT' || target.tagName == 'TEXTAREA') {
//         window.TLabWebViewActivity.unitySendMessage('SearchBar', 'OnMessage', 'Foucusin');
//     }
// }

// function focusout (e) {
//     const target = e.target;
//     if (target.tagName == 'INPUT' || target.tagName == 'TEXTAREA') {
//         window.TLabWebViewActivity.unitySendMessage('SearchBar', 'OnMessage', 'Foucusout');
//     }
// }

// for (var i = 0; i < elements.length; i++) {
//     elements[i].removeEventListener('focusin', focusin);
//     elements[i].removeEventListener('focusout', focusout);

//     elements[i].addEventListener('focusin', focusin);
//     elements[i].addEventListener('focusout', focusout);
// }