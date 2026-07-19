import type { MiddlewareConfig } from "@vercel/edge";

const GATEWAY_BASE_URL = "https://dotnetniger.runasp.net";

const BOT_RE =
  /facebookexternalhit|facebot|twitterbot|linkedinbot|whatsapp|slackbot|discordbot|telegrambot|pinterest|googlebot|bingbot|slurp|duckduckbot|baiduspider|yandexbot|skypeuripreview|vkshare|applebot|embedly/i;

const ROUTE_PATTERNS: {
  regex: RegExp;
  apiPath: (slug: string) => string;
  ogType: string;
}[] = [
  {
    regex: /^\/blog\/([^/]+)$/,
    apiPath: (slug) => `/api/posts/by-slug/${slug}`,
    ogType: "article",
  },
  {
    regex: /^\/evenements\/([^/]+)$/,
    apiPath: (slug) => `/api/events/by-slug/${slug}`,
    ogType: "website",
  },
  {
    regex: /^\/ressource\/([^/]+)$/,
    apiPath: (slug) => `/api/resources/by-slug/${slug}`,
    ogType: "website",
  },
  {
    regex: /^\/ressources\/([^/]+)$/,
    apiPath: (slug) => `/api/resources/by-slug/${slug}`,
    ogType: "website",
  },
];

const STATIC_EXT_RE =
  /\.(js|css|png|jpe?g|gif|svg|webp|woff2?|ttf|otf|eot|ico|wasm|dll|dat|json|txt|xml|pdf|zip|rar)$/i;

// --- Site settings cache ---
interface SiteSettings {
  siteName: string;
  defaultOgImage: string;
  logoUrl: string;
}

let cachedSettings: SiteSettings | null = null;
let settingsPromise: Promise<SiteSettings> | null = null;

async function fetchSettings(): Promise<SiteSettings> {
  try {
    const res = await fetch(`${GATEWAY_BASE_URL}/api/settings/public`, {
      headers: { Accept: "application/json" },
      signal: AbortSignal.timeout(3000),
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    const body = await res.json();
    if (!body.data) throw new Error("no data");
    const { siteName, defaultOgImage, logoUrl } = body.data;
    return {
      siteName: siteName || ".NET Niger",
      defaultOgImage: defaultOgImage || "/images/og-default.jpg",
      logoUrl: logoUrl || "",
    };
  } catch {
    return {
      siteName: ".NET Niger",
      defaultOgImage: "/images/og-default.jpg",
      logoUrl: "",
    };
  }
}

async function getSettings(): Promise<SiteSettings> {
  if (cachedSettings) return cachedSettings;
  if (!settingsPromise) settingsPromise = fetchSettings();
  cachedSettings = await settingsPromise;
  return cachedSettings!;
}

// ---

function escapeHtml(s: string): string {
  return s
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");
}

function buildOGPage(data: {
  title: string;
  description: string;
  image: string;
  url: string;
  ogType: string;
  siteName: string;
}): string {
  const t = escapeHtml(data.title);
  const d = escapeHtml(data.description);
  const img = escapeHtml(data.image);
  const u = escapeHtml(data.url);
  const sn = escapeHtml(data.siteName);

  return `<!DOCTYPE html>
<html lang="fr">
<head>
  <meta charset="utf-8">
  <title>${t} - ${sn}</title>
  <meta property="og:title" content="${t}" />
  <meta property="og:description" content="${d}" />
  <meta property="og:image" content="${img}" />
  <meta property="og:url" content="${u}" />
  <meta property="og:type" content="${data.ogType}" />
  <meta property="og:site_name" content="${sn}" />
  <meta property="og:locale" content="fr_FR" />
  <meta name="twitter:card" content="summary_large_image" />
  <meta name="twitter:title" content="${t}" />
  <meta name="twitter:description" content="${d}" />
  <meta name="twitter:image" content="${img}" />
</head>
<body>
  <h1>${t}</h1>
  <p>${d}</p>
  <noscript>Veuillez activer JavaScript pour afficher cette page.</noscript>
</body>
</html>`;
}

interface OgApiResponse {
  success: boolean;
  data?: { title: string; description: string; imageUrl: string };
}

export default async function middleware(request: Request) {
  const url = new URL(request.url);
  const ua = request.headers.get("user-agent") || "";

  if (STATIC_EXT_RE.test(url.pathname)) return;

  if (!BOT_RE.test(ua)) return;

  const settings = await getSettings();

  for (const route of ROUTE_PATTERNS) {
    const match = url.pathname.match(route.regex);
    if (!match) continue;

    const slug = match[1];
    const apiUrl = `${GATEWAY_BASE_URL}${route.apiPath(slug)}`;

    try {
      const res = await fetch(apiUrl, {
        headers: { Accept: "application/json" },
        signal: AbortSignal.timeout(5000),
      });

      if (!res.ok) break;

      const body: OgApiResponse = await res.json();
      if (!body.data) break;

      const { title, description, imageUrl } = body.data;
      const defaultImage = `${GATEWAY_BASE_URL}${settings.defaultOgImage}`;
      const image =
        imageUrl && !imageUrl.startsWith("http")
          ? `${GATEWAY_BASE_URL}${imageUrl}`
          : imageUrl || defaultImage;

      return new Response(
        buildOGPage({
          title: title || settings.siteName,
          description: description || "",
          image,
          url: url.href,
          ogType: route.ogType,
          siteName: settings.siteName,
        }),
        {
          headers: {
            "Content-Type": "text/html; charset=utf-8",
            "Cache-Control": "public, max-age=300, s-maxage=3600",
          },
        },
      );
    } catch {
      break;
    }
  }

  return;
}

export const config: MiddlewareConfig = {
  matcher: [
    "/((?!_framework|_next|api|.*\\.(?:js|css|png|jpe?g|gif|svg|webp|woff2?|ttf|otf|eot|ico|wasm|dll|dat|json|txt|xml|pdf|zip|rar)).*)",
  ],
};
