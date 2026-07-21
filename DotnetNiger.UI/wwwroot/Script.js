window.downloadFile = (fileName, mimeType, base64Content) => {
  const byteCharacters = atob(base64Content);
  const byteNumbers = new Array(byteCharacters.length);
  for (let i = 0; i < byteCharacters.length; i++) {
    byteNumbers[i] = byteCharacters.charCodeAt(i);
  }
  const byteArray = new Uint8Array(byteNumbers);
  const blob = new Blob([byteArray], { type: mimeType });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = fileName;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
};

window.getImageDimensionsFromStream = async (dotNetStreamRef) => {
  const arrayBuffer = await dotNetStreamRef.arrayBuffer();
  const blob = new Blob([arrayBuffer]);
  const url = URL.createObjectURL(blob);
  return new Promise((resolve, reject) => {
    const img = new Image();
    img.onload = () => {
      URL.revokeObjectURL(url);
      resolve({ width: img.width, height: img.height });
    };
    img.onerror = () => {
      URL.revokeObjectURL(url);
      reject("Format d'image invalide");
    };
    img.src = url;
  });
};

window.closeMenuOnOutsideClick = (dotnetHelper) => {
  document.addEventListener("click", function (event) {
    dotnetHelper.invokeMethodAsync("CloseMenu");
  });
};

window.triggerFileInput = (elementId) => {
  const el = document.getElementById(elementId);
  if (el) el.click();
};

window.initAvatarDropZone = (dropZoneId, inputId) => {
  const dropZone = document.getElementById(dropZoneId);
  const input = document.getElementById(inputId);

  if (
    !dropZone ||
    !input ||
    dropZone.dataset.avatarDropzoneInitialized === "true"
  ) {
    return;
  }

  dropZone.dataset.avatarDropzoneInitialized = "true";

  const setActiveState = (isActive) => {
    dropZone.classList.toggle("border-indigo-400", isActive);
    dropZone.classList.toggle("bg-indigo-50", isActive);
    dropZone.classList.toggle("ring-2", isActive);
    dropZone.classList.toggle("ring-indigo-200", isActive);
  };

  dropZone.addEventListener("dragenter", (event) => {
    event.preventDefault();
    setActiveState(true);
  });

  dropZone.addEventListener("dragover", (event) => {
    event.preventDefault();
    event.dataTransfer.dropEffect = "copy";
    setActiveState(true);
  });

  dropZone.addEventListener("dragleave", (event) => {
    if (event.target === dropZone) {
      setActiveState(false);
    }
  });

  dropZone.addEventListener("drop", (event) => {
    event.preventDefault();
    setActiveState(false);

    const file = event.dataTransfer?.files?.[0];
    if (!file) {
      return;
    }

    const dataTransfer = new DataTransfer();
    dataTransfer.items.add(file);
    input.files = dataTransfer.files;
    input.dispatchEvent(new Event("change", { bubbles: true }));
  });
};

window.dotnetNigerIsMobile = () =>
  window.matchMedia("(max-width: 767px)").matches;

window.openOAuthPopup = (authorizeUrl) => {
  const width = 600;
  const height = 700;
  const left = (screen.width - width) / 2;
  const top = (screen.height - height) / 2;
  const popup = window.open(
    authorizeUrl, "oauth-popup",
    "width=" + width + ",height=" + height + ",left=" + left + ",top=" + top + ",resizable=yes,scrollbars=yes"
  );

  return new Promise(function (resolve, reject) {
    if (!popup || popup.closed) {
      reject(new Error("Pop-up bloquée. Veuillez autoriser les pop-ups."));
      return;
    }

    var checkClosed = setInterval(function () {
      if (popup.closed) {
        clearInterval(checkClosed);
        window.removeEventListener("message", onMessage);
        reject(new Error("Fenêtre fermée par l'utilisateur"));
      }
    }, 500);

    function onMessage(event) {
      if (event.data && event.data.type === "oauth-callback") {
        clearInterval(checkClosed);
        window.removeEventListener("message", onMessage);
        if (event.data.code) {
          resolve(event.data.code);
        } else {
          reject(new Error(event.data.errorDescription || "Erreur d'authentification"));
        }
      }
    }

    window.addEventListener("message", onMessage);
  });
};
