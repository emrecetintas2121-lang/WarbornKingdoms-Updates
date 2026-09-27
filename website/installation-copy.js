const installationCopy = {
  en: {
    notice: 'The launcher verifies and installs our signed Europe1100 COOP Fixes module automatically. Steam Workshop mods remain managed by Steam.',
    launcherBody: 'Download and run the Warborn launcher. It automatically installs and updates only Europe1100 COOP Fixes; Steam Workshop mods stay managed by Steam.',
    pending: 'Download launcher for Windows x64'
  },
  tr: {
    notice: 'Launcher, imzalı Europe1100 COOP Fixes modumuzu otomatik doğrular ve kurar. Steam Workshop modlarını Steam yönetmeye devam eder.',
    launcherBody: 'Warborn launcher’ı indirip çalıştır. Yalnızca Europe1100 COOP Fixes modumuzu otomatik kurar ve günceller; Steam Workshop modlarını Steam yönetir.',
    pending: 'Windows x64 launcher’ı indir'
  },
  ru: {
    notice: 'Лаунчер автоматически проверяет и устанавливает наш подписанный мод Europe1100 COOP Fixes. Модами Steam Workshop управляет Steam.',
    launcherBody: 'Скачайте и запустите лаунчер Warborn. Он автоматически устанавливает и обновляет только Europe1100 COOP Fixes; модами Steam Workshop по-прежнему управляет Steam.',
    pending: 'Скачать лаунчер для Windows x64'
  }
};

function updateInstallationCopy(language) {
  const translations = installationCopy[language] || installationCopy.en;
  for (const [key, value] of Object.entries(translations)) {
    const element = document.querySelector(`[data-i18n="${key}"]`);
    if (element) element.textContent = value;
  }
}

document.querySelectorAll('[data-language]').forEach(button => {
  button.addEventListener('click', () => updateInstallationCopy(button.dataset.language));
});
updateInstallationCopy(document.documentElement.lang);
