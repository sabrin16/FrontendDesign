document.addEventListener('DOMContentLoaded', () => {
    showCookieModal();
    
    const consentValue = getCookie('cookieConsent');
    if (consentValue) {
        try {
            const consent = JSON.parse(consentValue);
            document.getElementById("cookieEssential").checked = consent.essential;
            document.getElementById("cookieFunctional").checked = consent.functional;
            document.getElementById("cookieAnalytics").checked = consent.analytics;
            document.getElementById("cookieMarketing").checked = consent.marketing;
        } catch (error) {
            console.error('Unable to handle cookie consent values', error);
        }
    }
});

function showCookieModal() {
    const modal = document.getElementById('cookieModal');
    if (modal) modal.style.display = "flex";
}

function hideCookieModal() {
    const modal = document.getElementById('cookieModal');
    if (modal) modal.style.display = "none";
}

function getCookie(name) {
    const nameEQ = name + "=";
    const cookies = document.cookie.split(';');
    for (let cookie of cookies) {
        cookie = cookie.trim();
        if (cookie.indexOf(nameEQ) === 0) {
            return decodeURIComponent(cookie.substring(nameEQ.length));
        }
    }
    return null;
}

function setCookie(name, value, days) {
    let expires = "";
    if (days) {
        const date = new Date();
        date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = `${name}=${encodeURIComponent(value || "")}${expires}; path=/; SameSite=Lax`;
}

async function handleConsent(consent) {
    try {
        const res = await fetch('/cookies/setcookies', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(consent)
        });
        if (!res.ok) {
            console.error("Unable to set cookie consent", await res.text());
        }
    } catch (error) {
        console.error("Error setting cookie consent:", error);
    }
}

async function acceptSelected() {
    const form = document.getElementById("cookieConsentForm");
    const formData = new FormData(form);
    const consent = {
        essential: formData.get("essential") === "on",
        functional: formData.get("functional") === "on",
        analytics: formData.get("analytics") === "on",
        marketing: formData.get("marketing") === "on"
    };
    setCookie("cookieConsent", JSON.stringify(consent), 365);
    await handleConsent(consent);
    hideCookieModal();
}

function reopenCookieModal() {
    const consentValue = getCookie('cookieConsent');
    if (!consentValue) return;
    try {
        const consent = JSON.parse(consentValue);
        document.getElementById("cookieEssential").checked = consent.essential;
        document.getElementById("cookieFunctional").checked = consent.functional;
        document.getElementById("cookieAnalytics").checked = consent.analytics;
        document.getElementById("cookieMarketing").checked = consent.marketing;
    } catch (error) {
        console.error('Unable to handle cookie consent values', error);
    }
    showCookieModal();
} 