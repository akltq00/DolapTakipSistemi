const lockerGrid = document.querySelector("#lockerGrid");
const assignDialog = document.querySelector("#assignDialog");
const assignForm = document.querySelector("#assignForm");
const selectedLockerNumber = document.querySelector("#selectedLockerNumber");
const closeDialog = document.querySelector("#closeDialog");
const cancelButton = document.querySelector("#cancelButton");
const formError = document.querySelector("#formError");
const releaseDialog = document.querySelector("#releaseDialog");
const releaseForm = document.querySelector("#releaseForm");
const releaseLockerNumber = document.querySelector("#releaseLockerNumber");
const closeReleaseDialog = document.querySelector("#closeReleaseDialog");
const cancelReleaseButton = document.querySelector("#cancelReleaseButton");
const releaseError = document.querySelector("#releaseError");
const helpDialog = document.querySelector("#helpDialog");
const helpButton = document.querySelector("#helpButton");
const closeHelpDialog = document.querySelector("#closeHelpDialog");

let selectedLockerId = null;

async function loadLockers() {
  const response = await fetch("/api/dolaplar");

  if (!response.ok) {
    lockerGrid.innerHTML = "<p>Dolaplar yuklenemedi. Veritabani baglantisini kontrol edin.</p>";
    return;
  }

  const lockers = await response.json();
  renderSummary(lockers);
  renderLockers(lockers);
}

function renderSummary(lockers) {
  const assignedCount = lockers.filter(locker => locker.zimmetliMi).length;

  document.querySelector("#totalCount").textContent = lockers.length;
  document.querySelector("#availableCount").textContent = lockers.length - assignedCount;
  document.querySelector("#assignedCount").textContent = assignedCount;
}

function renderLockers(lockers) {
  lockerGrid.innerHTML = "";

  lockers.forEach(locker => {
    const button = document.createElement("button");
    button.className = locker.zimmetliMi ? "locker assigned" : "locker";
    button.type = "button";
    button.innerHTML = `
      <span class="locker-number">${locker.numara}</span>
      <span class="locker-status">${locker.zimmetliMi ? locker.ogrenciAdSoyad : "Bos"}</span>
    `;

    button.addEventListener("click", () => {
      if (locker.zimmetliMi) {
        openReleaseDialog(locker);
        return;
      }

      openAssignDialog(locker);
    });

    lockerGrid.appendChild(button);
  });
}

function openAssignDialog(locker) {
  selectedLockerId = locker.id;
  selectedLockerNumber.textContent = locker.numara;
  formError.textContent = "";
  assignForm.reset();
  assignDialog.showModal();
  document.querySelector("#firstName").focus();
}

function openReleaseDialog(locker) {
  selectedLockerId = locker.id;
  releaseLockerNumber.textContent = locker.numara;
  releaseError.textContent = "";
  releaseForm.reset();
  releaseDialog.showModal();
  document.querySelector("#releasePassword").focus();
}

async function assignLocker(event) {
  event.preventDefault();
  formError.textContent = "";

  const formData = new FormData(assignForm);
  const payload = {
    ad: formData.get("firstName"),
    soyad: formData.get("lastName"),
    okulNumarasi: formData.get("studentNumber"),
    sifre: formData.get("password")
  };

  const response = await fetch(`/api/dolaplar/${selectedLockerId}/zimmete-al`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    formError.textContent = response.status === 409
      ? "Bu dolap az once zimmetlenmis."
      : "Kayit tamamlanamadi.";
    return;
  }

  assignDialog.close();
  await loadLockers();
}

async function releaseLocker(event) {
  event.preventDefault();
  releaseError.textContent = "";

  const formData = new FormData(releaseForm);
  const response = await fetch(`/api/dolaplar/${selectedLockerId}/zimmeti-kaldir`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ sifre: formData.get("password") })
  });

  if (!response.ok) {
    releaseError.textContent = response.status === 401
      ? "Sifre hatali."
      : "Zimmet kaldirilamadi.";
    return;
  }

  releaseDialog.close();
  await loadLockers();
}

assignForm.addEventListener("submit", assignLocker);
closeDialog.addEventListener("click", () => assignDialog.close());
cancelButton.addEventListener("click", () => assignDialog.close());
releaseForm.addEventListener("submit", releaseLocker);
closeReleaseDialog.addEventListener("click", () => releaseDialog.close());
cancelReleaseButton.addEventListener("click", () => releaseDialog.close());
helpButton.addEventListener("click", () => helpDialog.showModal());
closeHelpDialog.addEventListener("click", () => helpDialog.close());

loadLockers();
