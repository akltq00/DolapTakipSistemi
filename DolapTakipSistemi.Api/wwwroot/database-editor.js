const adminDialog = document.querySelector("#adminDialog");
const adminForm = document.querySelector("#adminForm");
const adminPassword = document.querySelector("#adminPassword");
const adminError = document.querySelector("#adminError");
const tableList = document.querySelector("#tableList");
const refreshTablesButton = document.querySelector("#refreshTablesButton");
const logoutButton = document.querySelector("#logoutButton");
const selectedTableTitle = document.querySelector("#selectedTableTitle");
const addRowButton = document.querySelector("#addRowButton");
const editorMessage = document.querySelector("#editorMessage");
const rowsContainer = document.querySelector("#rowsContainer");
const rowDialog = document.querySelector("#rowDialog");
const rowForm = document.querySelector("#rowForm");
const rowDialogTitle = document.querySelector("#rowDialogTitle");
const rowFields = document.querySelector("#rowFields");
const rowError = document.querySelector("#rowError");
const closeRowDialog = document.querySelector("#closeRowDialog");
const cancelRowButton = document.querySelector("#cancelRowButton");

let selectedTable = null;
let selectedColumns = [];
let selectedRows = [];
let editingRow = null;

function getAdminPassword() {
  return localStorage.getItem("dolap-admin-password") || "";
}

function setAdminPassword(password) {
  localStorage.setItem("dolap-admin-password", password);
}

async function adminFetch(url, options = {}) {
  const headers = {
    "X-Admin-Password": getAdminPassword(),
    ...(options.headers || {})
  };

  return fetch(url, { ...options, headers });
}

async function loadTables() {
  editorMessage.textContent = "";
  const response = await adminFetch("/api/admin/database/tables");

  if (response.status === 401) {
    adminDialog.showModal();
    adminPassword.focus();
    return;
  }

  if (!response.ok) {
    tableList.innerHTML = "<p>Tablolar yuklenemedi.</p>";
    return;
  }

  const tables = await response.json();
  renderTables(tables);
}

function renderTables(tables) {
  tableList.innerHTML = "";

  tables.forEach(table => {
    const button = document.createElement("button");
    button.type = "button";
    button.className = "table-item";
    button.textContent = `${table.schema}.${table.name}`;
    button.addEventListener("click", () => selectTable(table));
    tableList.appendChild(button);
  });
}

async function selectTable(table) {
  selectedTable = table;
  selectedTableTitle.textContent = `${table.schema}.${table.name}`;
  addRowButton.disabled = false;
  await loadRows();
}

async function loadRows() {
  if (!selectedTable) {
    return;
  }

  const response = await adminFetch(`/api/admin/database/tables/${encodeURIComponent(selectedTable.schema)}/${encodeURIComponent(selectedTable.name)}/rows`);

  if (!response.ok) {
    rowsContainer.innerHTML = "";
    editorMessage.textContent = response.status === 401
      ? "Admin sifresi gecersiz."
      : "Satirlar yuklenemedi.";
    return;
  }

  const payload = await response.json();
  selectedColumns = payload.columns;
  selectedRows = payload.rows;
  renderRows();
}

function renderRows() {
  if (selectedColumns.length === 0) {
    rowsContainer.innerHTML = "<p>Kolon bulunamadi.</p>";
    return;
  }

  const primaryKey = selectedColumns.find(column => column.isPrimaryKey);
  const headerCells = selectedColumns.map(column => `<th>${escapeHtml(column.name)}</th>`).join("");
  const bodyRows = selectedRows.map(row => {
    const cells = selectedColumns
      .map(column => `<td>${escapeHtml(formatValue(row[column.name]))}</td>`)
      .join("");
    const key = primaryKey ? row[primaryKey.name] : "";
    const actions = primaryKey
      ? `<td class="row-actions">
          <button type="button" data-action="edit" data-key="${escapeHtml(String(key))}">Duzenle</button>
          <button type="button" data-action="delete" data-key="${escapeHtml(String(key))}">Sil</button>
        </td>`
      : "<td></td>";

    return `<tr>${cells}${actions}</tr>`;
  }).join("");

  rowsContainer.innerHTML = `
    <div class="table-scroll">
      <table class="data-table">
        <thead><tr>${headerCells}<th>Islem</th></tr></thead>
        <tbody>${bodyRows || `<tr><td colspan="${selectedColumns.length + 1}">Satir yok.</td></tr>`}</tbody>
      </table>
    </div>
  `;

  rowsContainer.querySelectorAll("button[data-action]").forEach(button => {
    button.addEventListener("click", () => {
      const row = findRowByKey(button.dataset.key);

      if (!row) {
        return;
      }

      if (button.dataset.action === "edit") {
        openRowDialog(row);
        return;
      }

      deleteRow(row);
    });
  });
}

function findRowByKey(key) {
  const primaryKey = selectedColumns.find(column => column.isPrimaryKey);

  if (!primaryKey) {
    return null;
  }

  return selectedRows.find(row => String(row[primaryKey.name]) === key);
}

function openRowDialog(row = null) {
  editingRow = row;
  rowDialogTitle.textContent = row ? "Satir duzenle" : "Satir ekle";
  rowError.textContent = "";
  rowFields.innerHTML = "";

  selectedColumns
    .filter(column => !column.isIdentity && !(row && column.isPrimaryKey))
    .forEach(column => {
      const value = row ? row[column.name] : "";
      const label = document.createElement("label");
      label.textContent = `${column.name} (${column.dataType})`;
      const input = document.createElement("input");
      input.name = column.name;
      input.value = value ?? "";
      input.dataset.nullable = column.isNullable;
      input.dataset.type = column.dataType;
      label.appendChild(input);
      rowFields.appendChild(label);
    });

  rowDialog.showModal();
}

async function saveRow(event) {
  event.preventDefault();
  rowError.textContent = "";

  const payload = {};
  const formData = new FormData(rowForm);

  for (const [key, value] of formData.entries()) {
    payload[key] = value === "" ? null : coerceValue(key, value);
  }

  const primaryKey = selectedColumns.find(column => column.isPrimaryKey);
  const isEdit = Boolean(editingRow);
  const url = isEdit
    ? `/api/admin/database/tables/${encodeURIComponent(selectedTable.schema)}/${encodeURIComponent(selectedTable.name)}/rows/${encodeURIComponent(editingRow[primaryKey.name])}`
    : `/api/admin/database/tables/${encodeURIComponent(selectedTable.schema)}/${encodeURIComponent(selectedTable.name)}/rows`;

  const response = await adminFetch(url, {
    method: isEdit ? "PUT" : "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    rowError.textContent = "Kayit islemi tamamlanamadi.";
    return;
  }

  rowDialog.close();
  await loadRows();
}

async function deleteRow(row) {
  const primaryKey = selectedColumns.find(column => column.isPrimaryKey);

  if (!primaryKey || !confirm("Bu satir silinsin mi?")) {
    return;
  }

  const response = await adminFetch(
    `/api/admin/database/tables/${encodeURIComponent(selectedTable.schema)}/${encodeURIComponent(selectedTable.name)}/rows/${encodeURIComponent(row[primaryKey.name])}`,
    { method: "DELETE" });

  if (!response.ok) {
    editorMessage.textContent = "Satir silinemedi.";
    return;
  }

  await loadRows();
}

function coerceValue(key, value) {
  const column = selectedColumns.find(item => item.name === key);

  if (!column) {
    return value;
  }

  if (["int", "bigint", "smallint", "tinyint", "decimal", "numeric", "float", "real"].includes(column.dataType)) {
    return Number(value);
  }

  if (column.dataType === "bit") {
    return value === "true" || value === "1";
  }

  return value;
}

function formatValue(value) {
  if (value === null || value === undefined) {
    return "";
  }

  return String(value);
}

function escapeHtml(value) {
  return String(value)
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#039;");
}

adminForm.addEventListener("submit", async event => {
  event.preventDefault();
  adminError.textContent = "";
  setAdminPassword(adminPassword.value);
  adminDialog.close();
  await loadTables();
});

refreshTablesButton.addEventListener("click", loadTables);
logoutButton.addEventListener("click", () => {
  localStorage.removeItem("dolap-admin-password");
  adminDialog.showModal();
});
addRowButton.addEventListener("click", () => openRowDialog());
rowForm.addEventListener("submit", saveRow);
closeRowDialog.addEventListener("click", () => rowDialog.close());
cancelRowButton.addEventListener("click", () => rowDialog.close());

loadTables();
