# راه‌اندازی خروجی واقعی Android با Unity

این مسیر با APK پیش‌نمایش WebView فرق دارد و پروژه را با Unity Editor نسخه `2022.3.62f1` برای Android کامپایل می‌کند.

## گردش ساخت
- Workflow: `.github/workflows/build-unity-android.yml`
- نقطه ورود ساخت: `Assets/Editor/AncientAfghanistanBuild.cs`
- خروجی مورد انتظار: `Builds/Android/AncientAfghanistan.apk`
- فایل‌های کنترل صحت: `AncientAfghanistan.apk.sha256` و `build-info.txt` همراه APK در Artifact قرار می‌گیرند.
- معماری خروجی: Android ARM64، فرمت APK، حالت افقی
- صحنه نمونه در صورت نبودن، هنگام Build به‌صورت خودکار ایجاد می‌شود.

## پیش‌نیاز اجباری
GitHub Actions باید به مجوز معتبر Unity دسترسی داشته باشد. در تنظیمات مخزن، یک Repository Secret با نام دقیق `UNITY_LICENSE` لازم است که محتوای فایل مجوز فعال‌سازی Unity را در خود داشته باشد. این مقدار باید از حساب/مجوز قانونی Unity صاحب پروژه تهیه شود؛ این مخزن یا سازنده Workflow نمی‌تواند مجوز را جعل یا از طرف کاربر صادر کند.

پس از افزودن Secret:
1. به زبانه Actions مخزن بروید.
2. Workflow با نام **Build Unity Android APK** را انتخاب کنید.
3. گزینه **Run workflow** را روی شاخه `feature/gameplay-foundation` اجرا کنید.
4. فقط پس از موفقیت کامل، Artifact با نام `Afghanistan-Old-Unity-Android-APK` قابل دریافت است.

## محدودیت کیفیت این خروجی
این اولین Build واقعی Unity از صحنه و سیستم‌های رویه‌ای فعلی است؛ تضمین‌کننده کیفیت نهایی تجاری نیست. مخزن هنوز بسته کامل مدل‌های سه‌بعدی هنری، انیمیشن‌های نهایی، صداها و صحنه‌های محتوایی کامل را ندارد. خروجی باید روی گوشی واقعی آزمایش شود و خطاهای کامپایل/اجرا پیش از انتشار برطرف شوند.
