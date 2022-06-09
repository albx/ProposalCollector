function initializeCaptcha(dotNetObject, selector, sitekeyValue) {
    return grecaptcha.render(selector, {
        'sitekey': sitekeyValue,
        'callback': (response) => { dotNetObject.invokeMethodAsync('Success', response); },
        'expired-callback': () => { dotNetObject.invokeMethodAsync('Expired', response); }
    });
}

function getResponse(response) {
    return grecaptcha.getResponse(response);
}