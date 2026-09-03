// ── Real business contact details ───────────────────────────────────────────
//
// Single source of truth for anything the site publishes as a way to reach the
// shop. Both languages and the footer read from here, so a number can never be
// right in one place and stale in another.
//
// Every value below is copied from the Flutter app's contact screen, where the
// file states that each one is confirmed by the client:
//   mobile/nakhat_finjan/lib/screens/info/info_screens.dart:178-196
// Change it there and here together, never in only one.
export const CONTACT = {
  // The national form — how the shop writes the number, and how it is shown.
  phone: '0798912275',

  // The same number in E.164 for tel: links. A visitor abroad, or on a foreign
  // SIM, cannot dial the national form; the leading 0 is a domestic prefix that
  // does not exist outside Jordan.
  phoneDial: '+962798912275',

  // The same number again in the only shape wa.me accepts: country code, no
  // plus, and no leading 0 — wa.me silently fails on a national-form number.
  whatsApp: 'https://wa.me/962798912275',

  facebook:
    'https://www.facebook.com/nakhetfengan.coffe.and.chocolate/?locale=ar_AR',

  instagram: 'https://www.instagram.com/nakhet.fengan.coffee/',

  // The branch, as the app's map button opens it
  // (info_screens.dart:186-188). The embed below is the same pair of
  // coordinates in Google's keyless embed form, so the pin on the page and the
  // pin in the app are literally the same point.
  mapsUrl: 'https://maps.google.com/?q=32.025129,36.0585884',
  mapsEmbedUrl:
    'https://www.google.com/maps?q=32.025129,36.0585884&hl=ar&z=16&output=embed',

  // TODO: real value — the shop publishes no email address anywhere in the app
  // source, so there is nothing to copy here. While this is null the contact
  // card and the footer both leave the email row out entirely. Set it only to
  // an address the client confirms; never substitute a plausible-looking one.
  email: null,
}

export const translations = {
  ar: {
    nav: {
      home: 'الرئيسية',
      menu: 'المنيو',
      story: 'قصتنا',
      contact: 'التواصل',
      app: 'التطبيق',
      english: 'English',
    },
    footer: {
      description: 'نقدم لكم قهوة مصنوعة يدوياً، ولحظات دافئة كل يوم.',
      quickLinks: 'روابط سريعة',
      openingHours: 'ساعات العمل',
      contact: 'للتواصل',
      email: 'إيميل',
      whatsapp: 'واتساب',
      phone: 'رقم الهاتف',
      address: 'الرصيفة – طريق الياجوز، الزرقاء، الأردن',
      followUs: 'تابعونا',
      // One line, not a weekday/Friday split: the shop opens the same hours
      // every day (info_screens.dart:250-258).
      hoursDaily: 'يومياً: ١٠:٠٠ صباحاً – ١١:٠٠ مساءً',
    },
    home: {
      heroTitle: 'قهوة طازجة لكل مزاج',
      heroDescription: 'نكهات غنية وأجواء دافئة كل يوم.',
      menuButton: 'المنيو',
      locationButton: 'موقعنا',
      // REVIEW: this is marketing copy, not supplied product fact. It is written
      // to be appealing while avoiding anything checkable that nobody told us —
      // no roast level, no origin farm, no tasting notes presented as spec.
      // Reword freely; the shop knows its own beans better than this does.
      beansEyebrow: 'OUR BEANS',
      beansTitle: 'أجود أنواع البن',
      beansIntro:
        'حبوب مختارة بعناية ومحمّصة طازجة — كل نوع بنكهته وحكايته الخاصة.',
      // Names exactly as given — not translated or reworded.
      beans: [
        [
          'قهوة برازيلي',
          'من أشهر أنواع البن حول العالم. قوام غني وطعم متوازن بيمشي مع كل مزاج — بداية مثالية ليومك.',
        ],
        [
          'قهوة خصوصي',
          'نخبة حبوبنا، مختارة بعناية زيادة لعشّاق القهوة اللي بيعرفوا الفرق من أول رشفة.',
        ],
        [
          'قهوة جولد',
          'طعم ناعم ودافي وحضور مميز بكل فنجان — للحظات اللي بتستاهل إشي ألذ.',
        ],
      ],
      // Every line here describes something the app actually does. The screen
      // names are its four tabs (mobile/.../lib/widgets/app_bottom_nav.dart:9-17),
      // the order states are its three real ones
      // (mobile/.../lib/screens/purchases_screen.dart:15-31), and the rates come
      // from LoyaltyConstants — 3 points per dinar, 100 points per JOD, 200
      // minimum. Nothing is rounded up or dressed for marketing.
      getApp: {
        eyebrow: 'NAKHAT FINJAN APP',
        title: 'نقاطك معك، بكل زيارة',
        description:
          'حمّل تطبيق نكهة فنجان وتابع رصيد نقاطك، مشترياتك، والمنيو — كله من هاتفك، بدون كرت ورقي.',
        rate:
          'كل دينار بتدفعه نقداً بيعطيك ٣ نقاط. وكل ١٠٠ نقطة = ١ دينار خصم، وأقل استبدال ٢٠٠ نقطة.',
        features: [
          [
            'رصيدك وحركات نقاطك',
            'شاشة «الرئيسية» بتعرض رصيدك الحالي، وتحته كل حركة نقاط انضافت أو انخصمت.',
          ],
          [
            'المنيو والأسعار',
            'شاشة «المنيو» بتجيب الأصناف وأسعارها من المحل مباشرة — بالقطعة أو بالكيلو.',
          ],
          [
            'سجل مشترياتك',
            'شاشة «مشترياتي» بتوريك كل طلب وحالته — مكتمل، مُرتجع، أو ملغى — وكم نقطة كسبت منه.',
          ],
          [
            'دخول برقم هاتفك',
            'سجّل دخولك برقمك ورمز تحقق يوصلك برسالة. بدون كلمة مرور، ونقاطك مربوطة برقمك.',
          ],
        ],
        comingSoon: 'قريباً',
        playStorePrefix: 'حمّله من',
        playStoreName: 'Google Play',
        screensLabel: 'من داخل التطبيق',
        screens: ['الرئيسية', 'مشترياتي', 'المنيو'],
      },
      bestSellersTitle: 'الأكثر مبيعاً لدينا',
      // Badges for the featured items. Their names, prices, descriptions and
      // photos are not repeated here — all four are read from the menu itself
      // (see src/constants/bestSellers.js).
      bestSellerTags: { top: 'الأكثر مبيعاً', popular: 'ذات شعبية' },
    },
    about: {
      tag: 'قصتنا',
      title: 'حكاية بدأت من فكرة بسيطة',
      subtitle: 'مقهى حي صغير تحوّل إلى بيت ثانٍ لعشاق القهوة.',
      startedTag: 'كيف بدأنا',
      startedTitle: 'من زاوية صغيرة إلى وجهة يومية لعشاق القهوة',
      paragraphs: [
        'بدأت نكهة فنجان بحلم بسيط: تقديم كوب قهوة يشعر فيه الزائر بالراحة والاهتمام في كل تفصيلة. افتتحنا أول فرع لنا في زاوية هادئة من المدينة، بمعدات متواضعة وشغف كبير بحبوب البن المحمصة بعناية.',
        'مع كل كوب قدّمناه، كبرت قصتنا بفضل ثقة زوارنا. اليوم أصبحت نكهة فنجان مساحة يجتمع فيها الأصدقاء، ويعمل فيها الطلاب، ويبدأ فيها الكثيرون صباحهم — وما زلنا نحمّص كل حبة بنفس الحب الذي بدأنا به.',
      ],
      quote: 'كل ما بنيناه هنا بدأ بفكرة واحدة: أن نجعل فنجان القهوة لحظة يستحقها كل زائر.',
      founder: 'مؤسس نكهة فنجان',
    },
    contact: {
      title: 'نسعد بسماع رأيك',
      subtitle: 'سواء عندك سؤال، اقتراح، أو حجز مناسبة — فريقنا جاهز للرد عليك.',
      formTitle: 'أرسل لنا رسالة',
      name: 'الاسم الكامل',
      email: 'البريد الإلكتروني',
      subject: 'الموضوع',
      message: 'رسالتك',
      send: 'أرسل الرسالة',
      sending: 'جاري الإرسال...',
      success: 'تم إرسال رسالتك بنجاح.',
      failure: 'تعذر إرسال الرسالة. حاول مرة أخرى.',
      address: 'العنوان',
      phone: 'الهاتف والتواصل',
      hours: 'ساعات العمل',
      addressValue: 'الرصيفة – طريق الياجوز، الزرقاء، الأردن',
      hoursDaily: 'يومياً: ١٠:٠٠ صباحاً – ١١:٠٠ مساءً',
      mapTitle: 'موقع نكهة فنجان على الخريطة',
      mapLink: 'افتح في خرائط جوجل',
      whatsapp: 'واتساب',
      phoneValue: CONTACT.phone,
      whatsAppUrl: CONTACT.whatsApp,
      emailValue: CONTACT.email,
    },
    download: {
      eyebrow: 'ANDROID APP',
      title: 'حمّل تطبيق نكهة فنجان',
      // Same promise the home page makes, told once here rather than reworded.
      description:
        'تابع رصيد نقاطك، مشترياتك، والمنيو — كله من هاتفك، بدون كرت ورقي.',
      rate:
        'كل دينار بتدفعه نقداً بيعطيك ٣ نقاط. وكل ١٠٠ نقطة = ١ دينار خصم، وأقل استبدال ٢٠٠ نقطة.',
      cta: 'تحميل التطبيق',
      ctaPending: 'الملف قيد التجهيز',
      checking: 'جارِ التحقق من الملف...',
      pendingNote:
        'نسخة التطبيق قيد التجهيز وبتنزل هون قريباً. تابعنا وبنخبرك أول ما تصير جاهزة.',
      forAndroid: 'لأجهزة أندرويد',
      // The whole reason this page exists.
      stepsTitle: 'كيف تثبّت التطبيق؟',
      stepsLead:
        'التطبيق بينزل من موقعنا مباشرة، مش من متجر Google Play. عشان هيك أندرويد رح يسألك سؤال أمان بالنص — وهاد طبيعي تماماً وبيصير مع أي تطبيق بينزل من خارج المتجر. هاي الخطوات:',
      steps: [
        [
          'نزّل الملف',
          'اضغط زر «تحميل التطبيق» فوق. رح ينزل ملف اسمه ينتهي بـ ‎.apk‎ — هاد هو التطبيق.',
        ],
        [
          'إذا سألك المتصفح، اختر «احتفاظ»',
          'بعض المتصفحات بتسأل «هل تريد الاحتفاظ بهذا الملف؟». اضغط «احتفاظ» أو «موافق» عشان يكمل التنزيل.',
        ],
        [
          'افتح الملف',
          'من شريط الإشعارات بعد ما يخلص التنزيل، أو من مجلد «التنزيلات» في هاتفك.',
        ],
        [
          'أندرويد رح يعطيك تحذير — هاد متوقّع',
          'رح تطلعلك رسالة زي «لأسباب أمنية، جهازك غير مسموح له بتثبيت تطبيقات غير معروفة من هذا المصدر». هاي مش معناها إن التطبيق فيه فيروس — أندرويد بيقولها لكل ملف بينزل من برّا المتجر.',
        ],
        [
          'فعّل السماح من هذا المصدر',
          'اضغط «الإعدادات» من نفس الرسالة، وشغّل الخيار «السماح من هذا المصدر». بعدها ارجع للخلف.',
        ],
        [
          'اضغط «تثبيت»',
          'إذا طلعتلك شاشة Play Protect بتقول «إرسال للفحص»، فيك تختار «تثبيت على أي حال».',
        ],
        [
          'افتح التطبيق وسجّل دخولك',
          'بيوصلك رمز تحقق على رقمك، وبعدها نقاطك بتظهرلك على طول.',
        ],
      ],
      safetyTitle: 'ليش بيطلع التحذير؟',
      safetyBody:
        'لأن الملف بينزل من موقعنا مش من متجر Google Play. أندرويد بيحذّر من أي مصدر خارج المتجر بشكل تلقائي، بغض النظر عن محتوى الملف. لما ينزل التطبيق على المتجر رح يختفي هالتحذير.',
      requirement: 'يحتاج أندرويد 6.0 أو أحدث.',
    },
    menu: {
      title: 'قائمة قهوتنا المميزة',
      all: 'الكل',
      filterLabel: 'اختيار الصنف',
      itemsWord: 'صنف',
      categories: [
        { title: 'قهوة ساخنة', items: [
          ['سبانش لاتيه', '1.50 JD', 'إسبريسو وحليب مبخّر مع لمسة حليب مكثّف محلّى.'],
          ['لاتيه', '1.50 JD', 'إسبريسو مع حليب مبخّر وطبقة رغوة خفيفة.'],
          ['كابتشينو', '1.50 JD', 'ثلث إسبريسو، ثلث حليب، وثلث رغوة كثيفة.'],
          ['أمريكانو', '1.00 JD', 'إسبريسو مخفّف بالماء الساخن — أخف وأطول.'],
          ['V60', '1.50 JD', 'تقطير يدوي بفلتر ورقي، نكهة صافية وخفيفة.'],
          ['إسبريسو', '0.75 JD', 'شوت مركّز من البن المطحون طازجاً.'],
          ['دبل شوت إسبريسو', '1.00 JD', 'ضعف كمية الإسبريسو لطعم أقوى وأعمق.'],
          ['موكا دارك', '2.00 JD', 'إسبريسو وشوكولاتة داكنة مع حليب مبخّر.'],
          // TODO: السعر — غير معروف ومتروك فاضي عمداً. بطاقة المنيو بتخفي
          // خانة السعر لما تكون فاضية. عبّيه بالشكل '2.00 JD'.
          ['موكا وايت', '2.00 JD', 'إسبريسو وشوكولاتة بيضاء مع حليب مبخّر.'],
          ['بستاشيو لاتيه', '2.00 JD', 'إسبريسو وحليب مبخّر بنكهة الفستق.'],
          ['فلات وايت', '1.50 JD', 'إسبريسو مزدوج مع حليب مخملي ورغوة رقيقة.'],
          ['قهوة تركية', '0.50 JD', 'بن مطحون ناعم يُغلى ببطء برغوة دافئة.'],
          ['RED EYE', '1.50 JD', 'قهوة مفلترة مع شوت إسبريسو — طاقة مضاعفة.'],
        ]},
        { title: 'قهوة باردة', items: [
          ['آيس سبانش لاتيه', '1.50 JD', 'إسبريسو وحليب بارد مع حليب مكثّف على ثلج.'],
          ['آيس بستاشيو لاتيه', '2.00 JD', 'إسبريسو وحليب بارد بنكهة الفستق على ثلج.'],
          ['آيس لاتيه', '1.50 JD', 'إسبريسو وحليب بارد على ثلج — خفيف ومنعش.'],
          ['آيس موكا', '2.00 JD', 'إسبريسو وشوكولاتة وحليب بارد على ثلج.'],
          ['آيس وايت موكا', '2.00 JD', 'إسبريسو وشوكولاتة بيضاء وحليب بارد على ثلج.'],
          ['آيس كراميل مكياتو', '2.00 JD', 'حليب بارد وكراميل مع شوت إسبريسو فوقه.'],
          ['آيس أمريكانو', '1.00 JD', 'إسبريسو مع ماء بارد وثلج — منعش وقوي.'],
          ['آيس V60', '1.50 JD', 'قهوة مقطّرة يدوياً تُقدّم باردة على ثلج.'],
          ['فرابتشينو', '2.00 JD', 'قهوة وحليب وثلج مخفوقة بقوام كريمي.'],
        ]},
        { title: 'موهيتو', items: [
          ['بلو كارساو', '1.75 JD', 'نعنع وليمون مع شراب البرتقال الأزرق على ثلج.'],
          ['موهيتو', '1.75 JD', 'نعنع طازج وليمون على ثلج مجروش.'],
          ['فراولة', '1.75 JD', 'نعنع وليمون مع الفراولة — حلو ومنعش.'],
          ['مكس بيري', '1.75 JD', 'خليط توت أحمر مع نعنع وليمون على ثلج.'],
          ['بطيخ', '1.75 JD', 'بطيخ منعش مع نعنع ولمسة ليمون.'],
          ['مانجو', '1.75 JD', 'مانجو استوائي مع نعنع وليمون على ثلج.'],
          ['باشن فروت', '1.75 JD', 'باشن فروت حامض مع نعنع طازج وثلج.'],
        ]},
        { title: 'ميلك شيك', items: [
          ['باشينجو', '2.00 JD', 'باشن فروت ومانجو مخفوقان مع الحليب.'],
          ['ميلينجو', '2.00 JD', 'بطيخ ومانجو مخفوقان مع الحليب.'],
          ['فراولة', '2.00 JD', 'فراولة وحليب مخفوقان مع لمسة آيس كريم.'],
          ['مكس بيري', '2.00 JD', 'خليط التوت مع الحليب المخفوق — حلو وحامض.'],
          ['بطيخ', '2.00 JD', 'بطيخ طازج مخفوق مع الحليب والثلج.'],
          ['مانجو', '2.00 JD', 'مانجو وحليب مخفوقان لقوام كريمي غني.'],
          ['باشن فروت', '2.00 JD', 'باشن فروت وحليب مخفوق — منعش ومختلف.'],
        ]},
        { title: 'حلويات', items: [
          ['كوكيز', '0.50 JD', 'كوكيز طري بقطع الشوكولاتة، يُخبز يومياً.'],
          ['كرواسون', '1.00 JD', 'عجينة زبدة مورّقة، مقرمشة من الخارج.'],
          ['دونات', '1.00 JD', 'دونات طرية بطبقة سكرية — من بلانِت دونات.'],
          // TODO: السعر — غير معروف. البطاقة بتخفي خانة السعر لما تكون فاضية.
          ['كب كيك', '', 'كب كيك بكريمة ناعمة ولمسة ذهبية.'],
        ]},
      ],
    },
  },
  en: {
    nav: {
      home: 'Home',
      menu: 'Menu',
      story: 'Our Story',
      contact: 'Contact',
      app: 'App',
      arabic: 'العربية',
    },
    footer: {
      description: 'Handcrafted coffee and warm moments, every day.',
      quickLinks: 'Quick Links',
      openingHours: 'Opening Hours',
      contact: 'Contact Us',
      email: 'Email',
      whatsapp: 'WhatsApp',
      phone: 'Phone',
      address: 'Al-Rusaifa – Yajouz Road, Zarqa, Jordan',
      followUs: 'Follow Us',
      hoursDaily: 'Daily: 10:00 AM – 11:00 PM',
    },
    home: {
      heroTitle: 'Fresh coffee for every mood',
      heroDescription: 'Rich flavors and warm moments, every day.',
      menuButton: 'Menu',
      locationButton: 'Find Us',
      // REVIEW: marketing copy, same caveat as the Arabic side.
      beansEyebrow: 'OUR BEANS',
      beansTitle: 'Our finest beans',
      beansIntro:
        'Carefully selected beans, freshly roasted — each kind with its own character.',
      beans: [
        [
          'Brazilian coffee',
          'One of the best known beans in the world. Rich body and a balanced cup that suits any mood — a perfect way to start the day.',
        ],
        [
          'Specialty coffee',
          'The pick of our beans, chosen with extra care for the people who taste the difference in the first sip.',
        ],
        [
          'Gold coffee',
          'Smooth, warm, and unmistakable in every cup — for the moments that deserve something finer.',
        ],
      ],
      getApp: {
        eyebrow: 'NAKHAT FINJAN APP',
        title: 'Your points, on every visit',
        description:
          'Download the Nakhat Finjan app and follow your points balance, your purchases and the menu — all from your phone, with no paper card.',
        rate:
          'Every dinar you pay in cash earns 3 points. Every 100 points is 1 JOD off, and the smallest redemption is 200 points.',
        features: [
          [
            'Your balance and points history',
            'The Home screen shows your current balance, and under it every point added or deducted.',
          ],
          [
            'The menu and its prices',
            'The Menu screen pulls items and prices straight from the shop — by the piece or by the kilo.',
          ],
          [
            'A record of what you bought',
            'My Purchases shows every order and its state — completed, returned or cancelled — and the points it earned.',
          ],
          [
            'Sign in with your phone number',
            'Sign in with your number and a verification code. No password, and your points stay tied to your number.',
          ],
        ],
        comingSoon: 'Coming soon',
        playStorePrefix: 'Get it on',
        playStoreName: 'Google Play',
        screensLabel: 'Inside the app',
        screens: ['Home', 'My Purchases', 'Menu'],
      },
      bestSellersTitle: 'Our best sellers',
      bestSellerTags: { top: 'Best seller', popular: 'Popular' },
    },
    about: {
      tag: 'Our Story',
      title: 'A story that started with a simple idea',
      subtitle: 'A small neighborhood cafe became a second home for coffee lovers.',
      startedTag: 'How it started',
      startedTitle: 'From a small corner to a daily destination for coffee lovers',
      paragraphs: [
        'Nakhat Finjan began with a simple dream: to serve coffee that makes every visitor feel comfortable and cared for. We opened our first branch in a quiet corner of the city, with humble equipment and a deep passion for carefully roasted beans.',
        'With every cup we served, our story grew through the trust of our visitors. Today, Nakhat Finjan is a place where friends meet, students work, and many begin their mornings. We still roast every bean with the same love we started with.',
      ],
      quote: 'Everything we built here began with one idea: to make every cup of coffee a moment worth having.',
      founder: 'Founder of Nakhat Finjan',
    },
    contact: {
      title: 'We would love to hear from you',
      subtitle: 'Whether you have a question, suggestion, or event booking, our team is ready to help.',
      formTitle: 'Send us a message',
      name: 'Full name',
      email: 'Email address',
      subject: 'Subject',
      message: 'Your message',
      send: 'Send message',
      sending: 'Sending...',
      success: 'Your message was sent successfully.',
      failure: 'The message could not be sent. Please try again.',
      address: 'Address',
      phone: 'Phone and contact',
      hours: 'Opening hours',
      addressValue: 'Al-Rusaifa – Yajouz Road, Zarqa, Jordan',
      hoursDaily: 'Daily: 10:00 AM – 11:00 PM',
      mapTitle: 'Nakhat Finjan on the map',
      mapLink: 'Open in Google Maps',
      whatsapp: 'WhatsApp',
      phoneValue: CONTACT.phone,
      whatsAppUrl: CONTACT.whatsApp,
      emailValue: CONTACT.email,
    },
    download: {
      eyebrow: 'ANDROID APP',
      title: 'Download the Nakhat Finjan app',
      description:
        'Follow your points balance, your purchases and the menu — all from your phone, with no paper card.',
      rate:
        'Every dinar you pay in cash earns 3 points. Every 100 points is 1 JOD off, and the smallest redemption is 200 points.',
      cta: 'Download the app',
      ctaPending: 'The file is being prepared',
      checking: 'Checking for the file...',
      pendingNote:
        'The build is being prepared and will be here shortly. Follow us and we will let you know the moment it is ready.',
      forAndroid: 'For Android devices',
      stepsTitle: 'How to install it',
      stepsLead:
        'The app downloads straight from our site, not from the Google Play Store. Android will therefore show you a security prompt partway through — this is completely normal and happens with any app installed from outside the store. Here are the steps:',
      steps: [
        [
          'Download the file',
          'Tap the download button above. A file ending in .apk will download — that is the app.',
        ],
        [
          'If your browser asks, choose Keep',
          'Some browsers ask whether you want to keep the file. Tap Keep or OK to let the download finish.',
        ],
        [
          'Open the file',
          'From your notification bar once the download finishes, or from the Downloads folder on your phone.',
        ],
        [
          'Android will warn you — this is expected',
          'You will see something like "For your security, your phone is not allowed to install unknown apps from this source." This does not mean the app is unsafe — Android says it for every file installed from outside the store.',
        ],
        [
          'Allow installs from this source',
          'Tap Settings in that same prompt and turn on "Allow from this source", then go back.',
        ],
        [
          'Tap Install',
          'If a Play Protect screen offers to scan the app, you can choose "Install anyway".',
        ],
        [
          'Open the app and sign in',
          'A verification code is sent to your number, and your points appear straight away.',
        ],
      ],
      safetyTitle: 'Why does the warning appear?',
      safetyBody:
        'Because the file comes from our site rather than the Google Play Store. Android warns about any source outside the store automatically, regardless of what the file contains. The warning disappears once the app is published on the store.',
      requirement: 'Requires Android 6.0 or newer.',
    },
    menu: {
      title: 'Our special coffee menu',
      all: 'All',
      filterLabel: 'Filter by category',
      itemsWord: 'items',
      categories: [
        { title: 'Hot Coffee', items: [
          ['Spanish latte', '1.50 JD', 'Espresso and steamed milk with a touch of sweet condensed milk.'],
          ['Latte', '1.50 JD', 'Espresso with steamed milk and a light layer of foam.'],
          ['Cappuccino', '1.50 JD', 'One third espresso, one third milk, and one third rich foam.'],
          ['Americano', '1.00 JD', 'Espresso diluted with hot water: lighter and longer.'],
          ['V60', '1.50 JD', 'Hand-poured coffee through a paper filter, clean and light.'],
          ['Espresso', '0.75 JD', 'A concentrated shot of freshly ground coffee.'],
          ['Double espresso', '1.00 JD', 'Twice the espresso for a stronger, deeper flavor.'],
          ['Dark mocha', '2.00 JD', 'Espresso and dark chocolate with steamed milk.'],
          // TODO: price — unknown, deliberately left blank. The menu card
          // hides the price chip while it is empty. Fill in as '2.00 JD'.
          ['White mocha', '2.00 JD', 'Espresso and white chocolate with steamed milk.'],
          ['Pistachio latte', '2.00 JD', 'Espresso and steamed milk with pistachio flavor.'],
          ['Flat white', '1.50 JD', 'Double espresso with velvety milk and thin foam.'],
          ['Turkish coffee', '0.50 JD', 'Finely ground coffee slowly boiled with warm foam.'],
          ['RED EYE', '1.50 JD', 'Brewed coffee with an espresso shot for extra energy.'],
        ]},
        { title: 'Cold Coffee', items: [
          ['Iced Spanish latte', '1.50 JD', 'Espresso and cold milk with condensed milk over ice.'],
          ['Iced pistachio latte', '2.00 JD', 'Espresso and cold milk with pistachio flavor over ice.'],
          ['Iced latte', '1.50 JD', 'Espresso and cold milk over ice: light and refreshing.'],
          ['Iced mocha', '2.00 JD', 'Espresso, chocolate, and cold milk over ice.'],
          ['Iced white mocha', '2.00 JD', 'Espresso and white chocolate with cold milk over ice.'],
          ['Iced caramel macchiato', '2.00 JD', 'Cold milk and caramel with a shot of espresso on top.'],
          ['Iced Americano', '1.00 JD', 'Espresso with cold water and ice: refreshing and bold.'],
          ['Iced V60', '1.50 JD', 'Hand-poured coffee served cold over ice.'],
          ['Frappuccino', '2.00 JD', 'Coffee, milk, and ice blended into a creamy texture.'],
        ]},
        { title: 'Mojito', items: [
          ['Blue curacao', '1.75 JD', 'Mint and lemon with blue orange syrup over ice.'],
          ['Mojito', '1.75 JD', 'Fresh mint and lemon over crushed ice.'],
          ['Strawberry', '1.75 JD', 'Mint and lemon with strawberry: sweet and refreshing.'],
          ['Mixed berry', '1.75 JD', 'A red berry blend with mint and lemon over ice.'],
          ['Watermelon', '1.75 JD', 'Refreshing watermelon with mint and a touch of lemon.'],
          ['Mango', '1.75 JD', 'Tropical mango with mint and lemon over ice.'],
          ['Passion fruit', '1.75 JD', 'Tangy passion fruit with fresh mint and ice.'],
        ]},
        { title: 'Milkshake', items: [
          ['Passion mango', '2.00 JD', 'Passion fruit and mango blended with milk.'],
          ['Mango blend', '2.00 JD', 'Watermelon and mango blended with milk.'],
          ['Strawberry', '2.00 JD', 'Strawberry and milk blended with a touch of ice cream.'],
          ['Mixed berry', '2.00 JD', 'Mixed berries with milkshake: sweet and tangy.'],
          ['Watermelon', '2.00 JD', 'Fresh watermelon blended with milk and ice.'],
          ['Mango', '2.00 JD', 'Mango and milk blended into a rich, creamy texture.'],
          ['Passion fruit', '2.00 JD', 'Passion fruit and milkshake: refreshing and different.'],
        ]},
        { title: 'Desserts', items: [
          ['Cookies', '0.50 JD', 'Soft cookies with chocolate chips, baked daily.'],
          ['Croissant', '1.00 JD', 'Flaky butter pastry, crisp on the outside.'],
          ['Donut', '1.00 JD', 'Soft donut with a sugary topping from Planet Donut.'],
          // TODO: price — unknown. The card hides the price chip while empty.
          ['Cupcake', '', 'A cupcake with soft cream and a gold-leaf touch.'],
        ]},
      ],
    },
  },
}
