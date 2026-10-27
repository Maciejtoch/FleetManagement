(function () {
    const translations = {
        en: {
            dashboard: 'Dashboard',
            vehicles: 'Vehicles',
            users: 'Users',
            stops: 'Stops',
            reports: 'Reports',
            service: 'Service',
            newReport: 'New Report',
            reportsHistory: 'Reports History',
            darkMode: 'Dark mode',
            lightMode: 'Light mode',
            logout: 'Logout',
            language: 'Language',
            city: 'Default city',
            notes: 'Notes',
            backToList: 'Back to list',
            saveReport: 'Save report',
            details: 'Details',
            edit: 'Edit'
        },
        pl: {
            dashboard: 'Panel',
            vehicles: 'Pojazdy',
            users: 'Użytkownicy',
            stops: 'Przystanki',
            reports: 'Raporty',
            service: 'Serwis',
            newReport: 'Nowy raport',
            reportsHistory: 'Historia raportów',
            darkMode: 'Tryb ciemny',
            lightMode: 'Tryb jasny',
            logout: 'Wyloguj',
            language: 'Język',
            city: 'Domyślne miasto',
            notes: 'Notatki',
            backToList: 'Powrót do listy',
            saveReport: 'Zapisz raport',
            details: 'Szczegóły',
            edit: 'Edytuj'
        }
    };

    function applyLanguage(lang) {
        const selectedLang = translations[lang] ? lang : 'en';
        document.documentElement.lang = selectedLang;

        document.querySelectorAll('[data-i18n]').forEach(function (element) {
            const text = translations[selectedLang][element.dataset.i18n];
            if (text) {
                element.textContent = text;
            }
        });

        const languageSelect = document.getElementById('languageSelect');
        if (languageSelect) {
            languageSelect.value = selectedLang;
        }

        const themeToggle = document.getElementById('theme-toggle');
        if (themeToggle) {
            const label = themeToggle.querySelector('span');
            if (label) {
                label.textContent = document.body.classList.contains('dark-mode') ? translations[selectedLang].lightMode : translations[selectedLang].darkMode;
            }
        }
    }

    function initializeLanguageToggle() {
        const storedLanguage = localStorage.getItem('fleet-language') || 'en';
        const languageSelect = document.getElementById('languageSelect');

        applyLanguage(storedLanguage);

        if (languageSelect) {
            languageSelect.addEventListener('change', function () {
                const nextLang = this.value;
                localStorage.setItem('fleet-language', nextLang);
                applyLanguage(nextLang);
            });
        }
    }

    document.addEventListener('DOMContentLoaded', initializeLanguageToggle);
    window.fleetI18n = { applyLanguage, translations };
})();
