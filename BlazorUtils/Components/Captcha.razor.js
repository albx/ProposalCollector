//export function initializeCaptcha(dotNetObject, selector, sitekeyValue) {
//    return grecaptcha.render(selector, {
//        'sitekey': sitekeyValue,
//        'callback': (response) => { dotNetObject.invokeMethodAsync('Success', response); },
//        'expired-callback': () => { dotNetObject.invokeMethodAsync('Expired', response); }
//    });
//}

//export function getResponse(response) {
//    return grecaptcha.getResponse(response);
//}

export function init(siteKeyValue) {
    const script = document.createElement("script");
    script.id = "recaptcha";
    script.src = `https://www.google.com/recaptcha/api.js?render=${siteKeyValue}`;

    document.body.appendChild(script);
}

export function removeScript() {
    const script = document.getElementById("recaptcha");
    document.body.removeChild(script);
}

export async function execute(siteKeyValue) {
    return new Promise((resolve, reject) => {
        grecaptcha.ready(function () {
            grecaptcha
                .execute(siteKeyValue, { action: 'submit' })
                .then((token) => resolve(token))
                .err((error) => reject(error));
        });
    });
}