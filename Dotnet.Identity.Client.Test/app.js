import { identityEndpoints } from "./endpoints.js";

const STORAGE_KEY = "dotnet-identity-client";
const PROFILE_KEY = "dotnet-identity-client-profiles";
const DEFAULT_CONFIG = {
  baseUrl: "http://localhost:5075",
  apiVersion: "1.0",
  timeout: 30,
  bearerToken: "",
  refreshToken: "",
  apiKey: "",
  selectedEndpointId: "",
};

const dom = {
  baseUrl: document.getElementById("baseUrl"),
  apiVersion: document.getElementById("apiVersion"),
  timeout: document.getElementById("timeout"),
  bearerToken: document.getElementById("bearerToken"),
  refreshToken: document.getElementById("refreshToken"),
  apiKey: document.getElementById("apiKey"),
  endpointSearch: document.getElementById("endpointSearch"),
  endpointList: document.getElementById("endpointList"),
  method: document.getElementById("method"),
  pathInput: document.getElementById("pathInput"),
  pathParamContainer: document.getElementById("pathParamContainer"),
  pathParams: document.getElementById("pathParams"),
  addQuery: document.getElementById("addQueryParam"),
  queryParams: document.getElementById("queryParams"),
  addHeader: document.getElementById("addHeader"),
  headerParams: document.getElementById("headerParams"),
  bodyMode: document.getElementById("bodyMode"),
  bodyInput: document.getElementById("bodyInput"),
  fileWrapper: document.getElementById("fileBody"),
  fileInput: document.getElementById("fileInput"),
  fileHint: document.getElementById("fileHint"),
  jsonWrapper: document.getElementById("jsonBody"),
  resetBody: document.getElementById("resetBody"),
  requestForm: document.getElementById("requestForm"),
  sendButton: document.getElementById("sendButton"),
  copyRequest: document.getElementById("copyRequest"),
  feedback: document.getElementById("feedback"),
  responseMeta: document.getElementById("responseMeta"),
  responseBody: document.getElementById("responseBody"),
  responseHeaders: document.getElementById("responseHeaders"),
  responseTabs: document.querySelector(".response-tabs"),
  copyResponse: document.getElementById("copyResponse"),
  clearResponse: document.getElementById("clearResponse"),
  authBadge: document.getElementById("authBadge"),
  endpointTitle: document.getElementById("selectedEndpointTitle"),
  endpointDescription: document.getElementById("endpointDescription"),
  profileSelect: document.getElementById("profileSelect"),
  profileName: document.getElementById("profileName"),
  saveProfile: document.getElementById("saveProfile"),
  loadProfile: document.getElementById("loadProfile"),
  deleteProfile: document.getElementById("deleteProfile"),
  responseHistory: document.getElementById("responseHistory"),
};

const endpointMap = new Map();
identityEndpoints.forEach((group) => {
  group.items.forEach((item) => endpointMap.set(item.id, item));
});

const state = {
  config: loadConfig(),
  selectedEndpoint: null,
  queryParams: [],
  headerParams: [],
  pathParams: [],
  bodyTemplate: "",
  bodyMode: "json",
  profiles: loadProfiles(),
  history: [],
};

initialize();

function initialize() {
  applyConfigToInputs();
  attachBaseInputListeners();
  attachDynamicRowHandlers();
  attachFormHandlers();
  renderEndpointList();
  restoreSelection();
  renderProfiles();
  renderHistory();
  setResponse("", "");
}

function loadConfig() {
  try {
    const saved = localStorage.getItem(STORAGE_KEY);
    return saved ? { ...DEFAULT_CONFIG, ...JSON.parse(saved) } : { ...DEFAULT_CONFIG };
  } catch {
    return { ...DEFAULT_CONFIG };
  }
}

function loadProfiles() {
  try {
    const raw = localStorage.getItem(PROFILE_KEY);
    return raw ? JSON.parse(raw) : {};
  } catch {
    return {};
  }
}

function persistConfig(partial) {
  state.config = { ...state.config, ...partial };
  localStorage.setItem(STORAGE_KEY, JSON.stringify(state.config));
}

function persistProfiles() {
  localStorage.setItem(PROFILE_KEY, JSON.stringify(state.profiles));
}

function applyConfigToInputs() {
  dom.baseUrl.value = state.config.baseUrl;
  dom.apiVersion.value = state.config.apiVersion;
  dom.timeout.value = state.config.timeout;
  dom.bearerToken.value = state.config.bearerToken;
  dom.refreshToken.value = state.config.refreshToken;
  dom.apiKey.value = state.config.apiKey;
}

function attachBaseInputListeners() {
  const mappings = [
    { el: dom.baseUrl, key: "baseUrl" },
    { el: dom.apiVersion, key: "apiVersion" },
    { el: dom.timeout, key: "timeout", parse: (value) => Number(value) || DEFAULT_CONFIG.timeout },
    { el: dom.bearerToken, key: "bearerToken" },
    { el: dom.refreshToken, key: "refreshToken" },
    { el: dom.apiKey, key: "apiKey" },
  ];

  mappings.forEach(({ el, key, parse }) => {
    el.addEventListener("input", () => {
      const value = parse ? parse(el.value) : el.value;
      persistConfig({ [key]: value });
    });
  });

  dom.endpointSearch.addEventListener("input", () => renderEndpointList(dom.endpointSearch.value));
}

function attachDynamicRowHandlers() {
  dom.addQuery.addEventListener("click", () => {
    state.queryParams.push({ key: "", value: "" });
    renderKeyValueRows("query");
  });

  dom.addHeader.addEventListener("click", () => {
    state.headerParams.push({ key: "", value: "" });
    renderKeyValueRows("header");
  });

  dom.queryParams.addEventListener("input", handleKeyValueInput);
  dom.headerParams.addEventListener("input", handleKeyValueInput);
  dom.queryParams.addEventListener("click", handleKeyValueRemoval);
  dom.headerParams.addEventListener("click", handleKeyValueRemoval);
  dom.pathParams.addEventListener("input", (event) => {
    const target = event.target;
    if (!(target instanceof HTMLInputElement)) {
      return;
    }
    const index = Number(target.dataset.pathIndex);
    if (Number.isFinite(index) && state.pathParams[index]) {
      state.pathParams[index].value = target.value;
    }
  });
}

function attachFormHandlers() {
  dom.bodyMode.addEventListener("change", () => {
    state.bodyMode = dom.bodyMode.value;
    toggleBodyInputs();
  });

  dom.resetBody.addEventListener("click", () => {
    if (state.bodyMode === "file") {
      dom.fileInput.value = "";
    } else {
      dom.bodyInput.value = state.bodyTemplate || "";
    }
  });

  dom.requestForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    try {
      const request = buildRequest();
      await executeRequest(request);
    } catch (error) {
      setFeedback(error.message || "Erreur inconnue", "error");
    }
  });

  dom.copyRequest.addEventListener("click", () => {
    try {
      const request = buildRequest({ allowFormData: false });
      const curl = toCurl(request);
      navigator.clipboard.writeText(curl);
      setFeedback("Commande cURL copiée.", "success");
    } catch (error) {
      setFeedback(error.message, "error");
    }
  });

  dom.copyResponse.addEventListener("click", async () => {
    const text = dom.responseBody.textContent || "";
    if (!text) {
      return;
    }
    await navigator.clipboard.writeText(text);
    setFeedback("Body copié dans le presse-papiers.", "success");
  });

  dom.clearResponse.addEventListener("click", () => {
    setResponse("", "");
    setFeedback("", null);
  });

  dom.responseTabs.addEventListener("click", (event) => {
    const button = event.target.closest("button.tab");
    if (!button) {
      return;
    }
    const target = button.dataset.tab;
    dom.responseTabs.querySelectorAll(".tab").forEach((tab) => tab.classList.remove("active"));
    button.classList.add("active");
    dom.responseBody.hidden = target !== "body";
    dom.responseHeaders.hidden = target !== "headers";
    dom.responseHistory.hidden = target !== "history";
  });

  dom.endpointList.addEventListener("click", (event) => {
    const button = event.target.closest("button[data-endpoint-id]");
    if (!button) {
      return;
    }
    const endpoint = endpointMap.get(button.dataset.endpointId);
    if (endpoint) {
      loadEndpoint(endpoint);
    }
  });

  dom.saveProfile.addEventListener("click", () => {
    const name = (dom.profileName.value || "").trim();
    if (!name) {
      setFeedback("Donnez un nom de profil.", "error");
      return;
    }
    const snapshot = {
      baseUrl: dom.baseUrl.value.trim(),
      apiVersion: dom.apiVersion.value.trim(),
      bearerToken: dom.bearerToken.value.trim(),
      refreshToken: dom.refreshToken.value.trim(),
      apiKey: dom.apiKey.value.trim(),
    };
    state.profiles[name] = snapshot;
    persistProfiles();
    renderProfiles(name);
    setFeedback(`Profil "${name}" enregistré.`, "success");
  });

  dom.loadProfile.addEventListener("click", () => {
    const name = dom.profileSelect.value;
    if (!name || !state.profiles[name]) {
      setFeedback("Choisissez un profil à charger.", "error");
      return;
    }
    applyProfile(name);
    setFeedback(`Profil "${name}" chargé.`, "success");
  });

  dom.deleteProfile.addEventListener("click", () => {
    const name = dom.profileSelect.value;
    if (!name || !state.profiles[name]) {
      setFeedback("Aucun profil sélectionné.", "error");
      return;
    }
    delete state.profiles[name];
    persistProfiles();
    renderProfiles();
    setFeedback("Profil supprimé.", "success");
  });
}

function renderProfiles(selectedName) {
  const select = dom.profileSelect;
  select.innerHTML = "";
  const placeholder = document.createElement("option");
  placeholder.value = "";
  placeholder.textContent = "Choisir...";
  select.append(placeholder);

  Object.keys(state.profiles)
    .sort()
    .forEach((name) => {
      const opt = document.createElement("option");
      opt.value = name;
      opt.textContent = name;
      if (name === selectedName) {
        opt.selected = true;
      }
      select.append(opt);
    });
}

function applyProfile(name) {
  const profile = state.profiles[name];
  if (!profile) return;
  dom.profileSelect.value = name;
  dom.profileName.value = name;
  dom.baseUrl.value = profile.baseUrl ?? state.config.baseUrl;
  dom.apiVersion.value = profile.apiVersion ?? state.config.apiVersion;
  dom.bearerToken.value = profile.bearerToken ?? "";
  dom.refreshToken.value = profile.refreshToken ?? "";
  dom.apiKey.value = profile.apiKey ?? "";
  persistConfig({
    baseUrl: dom.baseUrl.value,
    apiVersion: dom.apiVersion.value,
    bearerToken: dom.bearerToken.value,
    refreshToken: dom.refreshToken.value,
    apiKey: dom.apiKey.value,
  });
}

function renderEndpointList(filterText = "") {
  const filter = filterText.trim().toLowerCase();
  dom.endpointList.innerHTML = "";

  identityEndpoints.forEach((group) => {
    const matches = group.items.filter((item) => {
      if (!filter) {
        return true;
      }
      return [item.label, item.description, item.path, item.method].some((value) =>
        value?.toLowerCase().includes(filter)
      );
    });

    if (!matches.length) {
      return;
    }

    const details = document.createElement("details");
    details.className = "endpoint-group";
    details.open = true;

    const summary = document.createElement("summary");
    summary.textContent = group.group;
    details.append(summary);

    matches.forEach((item) => {
      const button = document.createElement("button");
      button.type = "button";
      button.dataset.endpointId = item.id;
      button.className = "endpoint-btn";

      const left = document.createElement("div");
      left.className = "method";
      left.textContent = item.method;

      const right = document.createElement("div");
      right.className = "path";
      right.textContent = item.path;

      button.append(left, right);
      details.append(button);
    });

    dom.endpointList.append(details);
  });

  if (!dom.endpointList.childElementCount) {
    const empty = document.createElement("p");
    empty.className = "muted";
    empty.textContent = "Aucun endpoint ne correspond au filtre.";
    dom.endpointList.append(empty);
  }

  highlightSelectedEndpoint();
}

function highlightSelectedEndpoint() {
  const id = state.selectedEndpoint?.id;
  dom.endpointList.querySelectorAll("button[data-endpoint-id]").forEach((button) => {
    button.classList.toggle("active", button.dataset.endpointId === id);
  });
}

function restoreSelection() {
  if (state.config.selectedEndpointId) {
    const endpoint = endpointMap.get(state.config.selectedEndpointId);
    if (endpoint) {
      loadEndpoint(endpoint);
      return;
    }
  }
  const fallback = identityEndpoints[0]?.items[0];
  if (fallback) {
    loadEndpoint(fallback);
  }
}

function loadEndpoint(endpoint) {
  state.selectedEndpoint = endpoint;
  state.selectedEndpointId = endpoint.id;
  persistConfig({ selectedEndpointId: endpoint.id });

  dom.endpointTitle.textContent = `${endpoint.method} · ${endpoint.label}`;
  dom.endpointDescription.textContent = endpoint.description || "";
  dom.method.value = endpoint.method;
  dom.pathInput.value = endpoint.path;
  dom.bodyMode.value = endpoint.bodyMode || inferBodyMode(endpoint.method);
  state.bodyMode = dom.bodyMode.value;
  state.bodyTemplate = formatBodyTemplate(endpoint.body);
  dom.bodyInput.value = state.bodyTemplate;
  dom.fileInput.value = "";
  dom.fileHint.textContent = endpoint.fileHint || "";
  dom.fileInput.accept = endpoint.fileAccept || "image/*";

  state.queryParams = buildRowState(endpoint.query);
  state.headerParams = buildRowState(endpoint.headers);
  state.pathParams = buildPathParams(endpoint);

  renderKeyValueRows("query");
  renderKeyValueRows("header");
  renderPathParams();
  toggleBodyInputs();
  setAuthBadge(endpoint.auth);
  highlightSelectedEndpoint();
  setFeedback("", null);
}

function buildRowState(source) {
  if (!source) {
    return [];
  }
  if (Array.isArray(source)) {
    return source.map((item) => ({ key: item.key ?? "", value: item.value ?? "" }));
  }
  return Object.entries(source).map(([key, value]) => ({ key, value: String(value ?? "") }));
}

function buildPathParams(endpoint) {
  const explicit = endpoint.pathParams ?? [];
  const explicitMap = new Map(explicit.map((param) => [param.key, param]));
  const matches = endpoint.path.match(/\{([^}]+)\}/g) || [];
  const params = [];

  matches.forEach((token) => {
    const raw = token.slice(1, -1);
    if (raw.startsWith("version")) {
      return;
    }
    const key = raw.split(":")[0];
    if (params.some((param) => param.key === key)) {
      return;
    }
    const fromExplicit = explicitMap.get(key);
    params.push({
      key,
      label: fromExplicit?.label || key,
      value: fromExplicit?.sample || "",
    });
  });

  return params;
}

function renderPathParams() {
  dom.pathParams.innerHTML = "";
  if (!state.pathParams.length) {
    dom.pathParamContainer.hidden = true;
    return;
  }
  dom.pathParamContainer.hidden = false;
  state.pathParams.forEach((param, index) => {
    const pill = document.createElement("div");
    pill.className = "pill";
    const label = document.createElement("span");
    label.textContent = `${param.label}:`;
    const input = document.createElement("input");
    input.value = param.value || "";
    input.placeholder = param.sample || param.key;
    input.dataset.pathIndex = index;
    pill.append(label, input);
    dom.pathParams.append(pill);
  });
}

function renderKeyValueRows(type) {
  const container = type === "query" ? dom.queryParams : dom.headerParams;
  const rows = type === "query" ? state.queryParams : state.headerParams;
  container.innerHTML = "";

  rows.forEach((row, index) => {
    const wrapper = document.createElement("div");
    wrapper.className = "kv-row";

    const keyInput = document.createElement("input");
    keyInput.placeholder = type === "query" ? "clé (search, skip...)" : "Header (X-Correlation-ID)";
    keyInput.value = row.key;
    keyInput.dataset.rowScope = type;
    keyInput.dataset.rowIndex = index;
    keyInput.dataset.field = "key";

    const valueInput = document.createElement("input");
    valueInput.placeholder = "valeur";
    valueInput.value = row.value;
    valueInput.dataset.rowScope = type;
    valueInput.dataset.rowIndex = index;
    valueInput.dataset.field = "value";

    const remove = document.createElement("button");
    remove.type = "button";
    remove.dataset.rowScope = type;
    remove.dataset.rowIndex = index;
    remove.dataset.removeRow = "1";
    remove.textContent = "×";

    wrapper.append(keyInput, valueInput, remove);
    container.append(wrapper);
  });
}

function handleKeyValueInput(event) {
  const input = event.target;
  if (!(input instanceof HTMLInputElement)) {
    return;
  }
  const scope = input.dataset.rowScope;
  const index = Number(input.dataset.rowIndex);
  const field = input.dataset.field;
  if (!scope || !field || !Number.isFinite(index)) {
    return;
  }
  const rows = scope === "query" ? state.queryParams : state.headerParams;
  if (!rows[index]) {
    return;
  }
  rows[index][field] = input.value;
}

function handleKeyValueRemoval(event) {
  const button = event.target.closest("button[data-remove-row]");
  if (!button) {
    return;
  }
  const scope = button.dataset.rowScope;
  const index = Number(button.dataset.rowIndex);
  const rows = scope === "query" ? state.queryParams : state.headerParams;
  if (rows[index]) {
    rows.splice(index, 1);
    renderKeyValueRows(scope);
  }
}

function toggleBodyInputs() {
  const isFile = state.bodyMode === "file";
  const isNone = state.bodyMode === "none";
  dom.jsonWrapper.hidden = isFile || isNone;
  dom.fileWrapper.hidden = !isFile;
  if (isNone) {
    dom.bodyInput.value = "";
  } else if (!isFile && !dom.bodyInput.value && state.bodyTemplate) {
    dom.bodyInput.value = state.bodyTemplate;
  }
}

function inferBodyMode(method) {
  return ["GET", "DELETE"].includes(method?.toUpperCase()) ? "none" : "json";
}

function formatBodyTemplate(template) {
  if (template == null) {
    return "";
  }
  if (typeof template === "string") {
    return template;
  }
  return JSON.stringify(template, null, 2);
}

function setAuthBadge(type) {
  const map = {
    public: { label: "Public", className: "auth-chip auth-public" },
    user: { label: "Bearer requis", className: "auth-chip auth-user" },
    admin: { label: "Admin", className: "auth-chip auth-admin" },
    apiKey: { label: "X-API-Key", className: "auth-chip auth-apiKey" },
  };
  const meta = map[type] || map.public;
  dom.authBadge.innerHTML = "";
  const chip = document.createElement("span");
  chip.className = meta.className;
  chip.textContent = meta.label;
  dom.authBadge.append(chip);
}

function buildRequest(options = {}) {
  if (!state.selectedEndpoint) {
    throw new Error("Veuillez sélectionner un endpoint.");
  }

  const method = dom.method.value.toUpperCase();
  const relativePath = materializePath(dom.pathInput.value);
  const url = buildUrl(relativePath);
  const headers = new Headers();

  const headerRows = state.headerParams.filter((row) => row.key.trim());
  headerRows.forEach((row) => {
    headers.set(row.key.trim(), applyPlaceholders(row.value));
  });

  const authType = state.selectedEndpoint.auth;
  const bearer = dom.bearerToken.value.trim();
  if ((authType === "user" || authType === "admin") && bearer && !headers.has("Authorization")) {
    headers.set("Authorization", `Bearer ${bearer}`);
  }
  if (authType === "apiKey") {
    const apiKey = dom.apiKey.value.trim();
    if (!apiKey) {
      throw new Error("Une clé API est requise pour cet endpoint.");
    }
    if (!headers.has("X-API-Key")) {
      headers.set("X-API-Key", apiKey);
    }
  }

  const queryString = buildQueryString();
  const fullUrl = queryString ? `${url}${url.includes("?") ? "&" : "?"}${queryString}` : url;

  let body = null;
  let isFormData = false;

  if (!["GET", "HEAD"].includes(method) && state.bodyMode !== "none") {
    if (state.bodyMode === "file") {
      if (!dom.fileInput.files.length) {
        throw new Error("Sélectionnez un fichier à envoyer.");
      }
      if (options.allowFormData === false) {
        throw new Error("La génération cURL n'est pas disponible pour un upload de fichier.");
      }
      const formData = new FormData();
      const fieldName = state.selectedEndpoint.fileField || "file";
      formData.append(fieldName, dom.fileInput.files[0]);
      body = formData;
      isFormData = true;
    } else {
      const payload = parseJsonBody();
      body = JSON.stringify(payload);
      if (!headers.has("Content-Type")) {
        headers.set("Content-Type", "application/json");
      }
    }
  }

  return {
    endpoint: state.selectedEndpoint,
    method,
    url: fullUrl,
    headers,
    body,
    isFormData,
    timeout: Number(dom.timeout.value) * 1000 || DEFAULT_CONFIG.timeout * 1000,
  };
}

function materializePath(template) {
  let path = template || "/";
  const version = dom.apiVersion.value.trim() || DEFAULT_CONFIG.apiVersion;
  path = path.replaceAll("{version}", version).replaceAll("{version:apiVersion}", version);
  state.pathParams.forEach((param) => {
    const token = `{${param.key}}`;
    path = path.replaceAll(token, param.value || token);
  });
  return path;
}

function buildUrl(relativePath) {
  const trimmedBase = dom.baseUrl.value.trim().replace(/\/$/, "");
  if (!trimmedBase) {
    throw new Error("Base URL invalide.");
  }
  if (/^https?:\/\//i.test(relativePath)) {
    return relativePath;
  }
  const normalized = relativePath.startsWith("/") ? relativePath : `/${relativePath}`;
  return `${trimmedBase}${normalized}`;
}

function buildQueryString() {
  const params = state.queryParams.filter((row) => row.key.trim());
  const usp = new URLSearchParams();
  params.forEach((row) => {
    usp.append(row.key.trim(), applyPlaceholders(row.value));
  });
  return usp.toString();
}

function parseJsonBody() {
  const raw = dom.bodyInput.value.trim();
  if (!raw) {
    return {};
  }
  try {
    const parsed = JSON.parse(raw);
    return applyPlaceholdersDeep(parsed);
  } catch {
    throw new Error("JSON invalide dans le corps de requête.");
  }
}

function applyPlaceholders(value) {
  if (typeof value !== "string") {
    return value;
  }
  return value.replace(/\{\{(.*?)\}\}/g, (_, token) => {
    const key = token.trim();
    switch (key) {
      case "refreshToken":
        return dom.refreshToken.value.trim();
      case "bearerToken":
        return dom.bearerToken.value.trim();
      case "apiKey":
        return dom.apiKey.value.trim();
      default:
        return "";
    }
  });
}

function applyPlaceholdersDeep(value) {
  if (Array.isArray(value)) {
    return value.map((item) => applyPlaceholdersDeep(item));
  }
  if (value && typeof value === "object") {
    return Object.fromEntries(
      Object.entries(value).map(([key, val]) => [key, applyPlaceholdersDeep(val)])
    );
  }
  return applyPlaceholders(value);
}

async function executeRequest(request) {
  setFeedback("Envoi en cours...", null);
  toggleLoading(true);
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), request.timeout);
  const init = {
    method: request.method,
    headers: request.headers,
    body: request.body,
    signal: controller.signal,
  };

  const start = performance.now();
  try {
    const response = await fetch(request.url, init);
    const elapsed = performance.now() - start;
    const rawText = await response.text();
    renderResponse(response, rawText, elapsed);
    pushHistory({
      method: request.method,
      path: request.endpoint?.path || request.url,
      status: response.status,
      ok: response.ok,
      elapsed,
    });
    if (!response.ok) {
      setFeedback(`Réponse ${response.status} ${response.statusText}`, "error");
    } else {
      setFeedback("Requête réussie.", "success");
    }
  } catch (error) {
    const message = error.name === "AbortError" ? "Requête annulée (timeout)." : error.message;
    setResponse("", "");
    setFeedback(message, "error");
  } finally {
    clearTimeout(timer);
    toggleLoading(false);
  }
}

function renderResponse(response, bodyText, elapsed) {
  const size = new TextEncoder().encode(bodyText).length;
  dom.responseMeta.textContent = `Status ${response.status} · ${elapsed.toFixed(1)} ms · ${size} o`;
  const parsed = tryParseJson(bodyText);
  dom.responseBody.textContent = parsed ? JSON.stringify(parsed, null, 2) : bodyText;
  const headers = [];
  response.headers.forEach((value, key) => headers.push(`${key}: ${value}`));
  dom.responseHeaders.textContent = headers.join("\n");
  dom.responseTabs.querySelectorAll(".tab").forEach((tab, index) => {
    tab.classList.toggle("active", index === 0);
  });
  dom.responseBody.hidden = false;
  dom.responseHeaders.hidden = true;
  dom.responseHistory.hidden = true;
}

function pushHistory(entry) {
  const next = [{ ...entry, ts: new Date().toISOString() }, ...state.history].slice(0, 8);
  state.history = next;
  renderHistory();
}

function renderHistory() {
  const container = dom.responseHistory;
  container.innerHTML = "";
  if (!state.history.length) {
    container.textContent = "Pas d'historique pour le moment.";
    return;
  }

  state.history.forEach((item) => {
    const row = document.createElement("div");
    row.className = "history-item";

    const method = document.createElement("span");
    method.textContent = item.method;
    method.className = "badge";

    const path = document.createElement("span");
    path.className = "history-path";
    path.textContent = item.path;

    const status = document.createElement("span");
    status.className = `badge ${item.ok ? "ok" : "fail"}`;
    status.textContent = `${item.status} · ${item.elapsed.toFixed(0)} ms`;

    row.append(method, path, status);
    container.append(row);
  });
}

function setResponse(bodyText, metaText) {
  dom.responseBody.textContent = bodyText;
  dom.responseHeaders.textContent = "";
  dom.responseMeta.textContent = metaText;
}

function toCurl(request) {
  if (request.isFormData) {
    throw new Error("Générez la commande manuellement pour les uploads multipart.");
  }
  const parts = [`curl -X ${request.method}`];
  request.headers.forEach((value, key) => {
    parts.push(`  -H "${key}: ${value}"`);
  });
  if (request.body) {
    const escaped = request.body.replace(/"/g, '\\"');
    parts.push(`  --data-raw "${escaped}"`);
  }
  parts.push(`  "${request.url}"`);
  return parts.join(" \\\n");
}

function tryParseJson(text) {
  if (!text) {
    return null;
  }
  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}

function toggleLoading(isLoading) {
  dom.sendButton.disabled = isLoading;
  dom.sendButton.textContent = isLoading ? "Envoi..." : "Envoyer la requête";
}

function setFeedback(message, tone) {
  dom.feedback.textContent = message;
  dom.feedback.classList.remove("feedback-error", "feedback-success");
  if (!tone) {
    return;
  }
  dom.feedback.classList.add(tone === "error" ? "feedback-error" : "feedback-success");
}
