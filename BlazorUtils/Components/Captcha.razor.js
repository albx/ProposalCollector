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

export function execute(siteKeyValue) {
    return new Promise((resolve, reject) => {
        grecaptcha.ready(function () {
            grecaptcha
                .execute(siteKeyValue, { action: 'submit' })
                .then((token) => resolve(token))
                .err((error) => reject(error));
        });
    });
}