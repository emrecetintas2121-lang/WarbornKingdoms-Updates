namespace WarbornLauncher;
internal static class Locale
{
 public static string Current="en";
 private static readonly Dictionary<string,string[]> Text=new()
 {
  ["readyLabel"]=["WARBORN / EUROPE1100","WARBORN / EUROPE1100","WARBORN / EUROPE1100"],
  ["updateAction"]=["INSTALL / UPDATE","KUR / GÜNCELLE","УСТАНОВИТЬ / ОБНОВИТЬ"],
  ["joinAction"]=["JOIN WARBORN","WARBORN'A KATIL","ВОЙТИ В WARBORN"],
  ["campaignLabel"]=["THE WARBORN CAMPAIGN","WARBORN KAMPANYASI","КАМПАНИЯ WARBORN"],
  ["campaignLine"]=["Mount & Blade II: Bannerlord · Cooperative campaign","Mount & Blade II: Bannerlord · Ortak kampanya","Mount & Blade II: Bannerlord · Совместная кампания"],
  ["profileLabel"]=["EUROPE1100 PROFILE","EUROPE1100 PROFİLİ","ПРОФИЛЬ EUROPE1100"],
  ["hub"]=["CAMPAIGN HUB","KAMPANYA MERKEZİ","ЦЕНТР КАМПАНИИ"],
  ["home"]=["⌂   Overview","⌂   Genel bakış","⌂   Обзор"], ["mods"]=["▦   Mod library","▦   Mod kütüphanesi","▦   Библиотека модов"],
  ["updates"]=["↻   Updates","↻   Güncellemeler","↻   Обновления"], ["folder"]=["⚙   Game directory","⚙   Oyun klasörü","⚙   Папка игры"],
  ["website"]=["↗   Official website","↗   Resmî web sitesi","↗   Официальный сайт"], ["language"]=["LANGUAGE","DİL","ЯЗЫК"],
  ["apply"]=["APPLY TO GAME","OYUNA UYGULA","ПРИМЕНИТЬ В ИГРЕ"], ["preview"]=["LOCAL DESIGN PREVIEW","YEREL TASARIM ÖNİZLEMESİ","ЛОКАЛЬНЫЙ ПРОТОТИП"],
  ["hero"]=["Forge your legacy.","Kendi tarihini yaz.","Создай свою легенду."],
  ["subtitle"]=["A shared medieval world. Your clan, your allies, your story.\nPrepare your campaign with Warborn Kingdoms.","Ortak bir orta çağ dünyası. Klanın, müttefiklerin, hikâyen.\nWarborn Kingdoms ile kampanyaya hazırlan.","Общий средневековый мир. Твой клан, союзники и история.\nПодготовься к кампании Warborn Kingdoms."],
  ["prepare"]=["PREPARE YOUR CAMPAIGN  →","KAMPANYAYA HAZIRLAN  →","ПОДГОТОВИТЬ КАМПАНИЮ  →"],
  ["safe"]=["Steam mods stay managed by Steam","Steam modlarını Steam yönetir","Модами Steam управляет Steam"],
  ["unknown"]=["●  Live status not connected","●  Canlı durum henüz bağlı değil","●  Статус сервера не подключён"],
  ["check"]=["↻   Check installation","↻   Kurulumu kontrol et","↻   Проверить установку"],
  ["installTitle"]=["Installation","Kurulum","Установка"],
  ["owned"]=["Warborn mods","Warborn modları","Моды Warborn"],
  ["crafted"]=["Crafted for our campaign","Kampanyamız için hazırlandı","Создано для нашей кампании"],
  ["ownedDesc"]=["Only our own packages will be updated by this launcher. Your Workshop files stay untouched.","Launcher yalnız bizim paketleri güncelleyecek. Workshop dosyalarına dokunulmaz.","Лаунчер обновляет только наши пакеты. Файлы Workshop не изменяются."],
  ["upcoming"]=["Automatic updates are not connected yet. This build checks your installation only.","Otomatik güncelleme henüz bağlı değil. Bu sürüm yalnız kurulumunu kontrol eder.","Автообновление пока не подключено. Эта версия только проверяет установку."],
  ["steamDesc"]=["Subscribe on Steam. We check what is installed.","Steam'den abone ol. Biz kurulumu kontrol ediyoruz.","Подпишись в Steam. Мы проверим установку."],
  ["installed"]=["INSTALLED","KURULU","УСТАНОВЛЕНО"], ["missing"]=["MISSING","EKSİK","НЕ УСТАНОВЛЕНО"],
  ["local"]=["local manifest","yerel manifest","локальный манифест"], ["subscribe"]=["Steam subscription required","Steam aboneliği gerekli","Требуется подписка в Steam"],
  ["steam"]=["GET ON STEAM","STEAM'DE AÇ","ОТКРЫТЬ В STEAM"], ["manifestError"]=["Cannot read mod manifest","Mod manifesti okunamadı","Не удалось прочитать манифест"],
  ["noVersion"]=["No version specified","Sürüm belirtilmemiş","Версия не указана"],
  ["checked"]=["Local check","Yerel kontrol","Локальная проверка"], ["warning"]=["Installed does not mean version-compatible.","Kurulu olması sürüm uyumunu kanıtlamaz.","Установка не гарантирует совместимость версий."],
  ["browse"]=["Choose the Bannerlord game directory","Bannerlord oyun klasörünü seç","Выберите папку Bannerlord"],
  ["badFolder"]=["Bannerlord.exe not found. Selection was not applied.","Bannerlord.exe bulunamadı; seçim uygulanmadı.","Bannerlord.exe не найден. Папка не выбрана."],
  ["linkError"]=["Could not open the link. Check your default browser.","Bağlantı açılamadı; varsayılan tarayıcıyı kontrol et.","Не удалось открыть ссылку. Проверьте браузер."],
  ["homeTitle"]=["Your campaign starts here.","Kampanyan burada başlıyor.","Здесь начинается твоя кампания."],
  ["homeBody"]=["Install the Steam requirements, then verify the Warborn collection.\n\nLive game status and Discord account linking are still in development. No invented player counts are displayed.","Steam gereksinimlerini kur, ardından Warborn koleksiyonunu kontrol et.\n\nCanlı oyun durumu ve Discord hesap bağlantısı geliştiriliyor. Gerçek olmayan oyuncu sayısı gösterilmez.","Установи моды Steam, затем проверь коллекцию Warborn.\n\nСтатус игры и привязка Discord пока в разработке. Выдуманное число игроков не отображается."],
  ["updateBody"]=["Signed manifests, hash checks and rollback are next. This design build does not install mod files.\n\nWorkshop packages remain managed by Steam.","İmzalı manifest, hash kontrolü ve geri dönüş sonraki aşama. Bu tasarım sürümü mod kurmaz.\n\nWorkshop paketlerini Steam yönetir.","Подписанные манифесты, проверка хешей и откат — следующий этап. Этот прототип не устанавливает моды.\n\nПакетами Workshop управляет Steam."],
  ["languageSaved"]=["Game text language saved. Takes effect on the next game start.","Oyun metin dili kaydedildi. Sonraki açılışta uygulanır.","Язык текста сохранён. Применится при следующем запуске игры."],
  ["languageBusy"]=["Close Bannerlord before applying its language.","Dili uygulamak için önce Bannerlord'u kapat.","Закройте Bannerlord перед изменением языка."],
  ["configMissing"]=["Game configuration not found. Start Bannerlord once first.","Oyun ayarı bulunamadı. Önce Bannerlord'u bir kez aç.","Настройки игры не найдены. Сначала запустите Bannerlord."],
  ["saveError"]=["Could not save the language setting. Existing configuration was preserved.","Dil ayarı kaydedilemedi. Mevcut ayar korundu.","Не удалось сохранить язык. Исходные настройки сохранены."]
 };
 public static string Get(string key)=>Text.TryGetValue(key,out var values)?values[Current=="tr"?1:Current=="ru"?2:0]:key;
 public static string GameLanguage=>Current=="tr"?"Türkçe":Current=="ru"?"Русский":"English";
}
