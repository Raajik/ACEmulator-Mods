(() => {
    const state = {
        spells: [],
        sets: [],
        selectedSpellIndex: null,
        selectedSetIndex: null,
        selectedTierIndex: null,
        metadata: {
            spellsById: {},
            spellsByEnum: {},
            allSpells: [],
            schools: {},
            elements: {}
        }
    };

    const dom = {};

    document.addEventListener("DOMContentLoaded", () => {
        cacheDom();
        initTheme();
        initTabs();
        initEvents();
        loadMetadata();
        updateUiState();
    });

    function cacheDom() {
        dom.themeToggle = document.getElementById("themeToggle");
        dom.configFileInput = document.getElementById("configFileInput");
        dom.downloadConfigBtn = document.getElementById("downloadConfigBtn");
        dom.addSpellBtn = document.getElementById("addSpellBtn");
        dom.cloneSpellBtn = document.getElementById("cloneSpellBtn");
        dom.deleteSpellBtn = document.getElementById("deleteSpellBtn");
        dom.addSetBtn = document.getElementById("addSetBtn");
        dom.addTierBtn = document.getElementById("addTierBtn");
        dom.deleteTierBtn = document.getElementById("deleteTierBtn");
        dom.searchInput = document.getElementById("searchInput");
        dom.spellsTableBody = document.querySelector("#spellsTable tbody");
        dom.setsTableBody = document.querySelector("#setsTable tbody");
        dom.spellStatus = document.getElementById("spellStatus");
        dom.setStatus = document.getElementById("setStatus");
        dom.spellDetail = document.getElementById("spellDetail");
        dom.setDetail = document.getElementById("setDetail");
        dom.tabs = Array.from(document.querySelectorAll(".tab"));
        dom.tabPanels = {
            spells: document.getElementById("tab-spells"),
            sets: document.getElementById("tab-sets")
        };
    }

    function loadMetadata() {
        // Prefer full AllSpells metadata (with icons), fall back to simple spell metadata
        fetch("AllSpells.json")
            .then(response => {
                if (!response.ok) {
                    return fetch("AllSpells.sample.json");
                }
                return response;
            })
            .then(response => {
                if (!response || !response.ok) {
                    return fetch("SpellMetadata.json");
                }
                return response;
            })
            .then(response => {
                if (!response || !response.ok) {
                    return fetch("SpellMetadata.sample.json");
                }
                return response;
            })
            .then(response => {
                if (!response || !response.ok) {
                    return null;
                }
                return response.json();
            })
            .then(json => {
                if (!json) return;

                const spells = Array.isArray(json.spells) ? json.spells : [];
                const byId = {};
                const byEnum = {};

                for (const s of spells) {
                    if (s.id != null) {
                        byId[String(s.id)] = s;
                    }
                    if (s.spellId) {
                        byEnum[String(s.spellId)] = s;
                    }
                }

                state.metadata.spellsById = byId;
                state.metadata.spellsByEnum = byEnum;
                state.metadata.allSpells = spells;
                state.metadata.schools = json.schools || {};
                state.metadata.elements = json.elements || {};
            })
            .catch(() => {
                // Ignore metadata load failures
            });
    }

    function initTheme() {
        const root = document.documentElement;
        const storedTheme = window.localStorage.getItem("aethericweaver-theme");
        let theme = storedTheme || "dark";

        if (theme !== "dark" && theme !== "light") {
            theme = "dark";
        }

        root.setAttribute("data-theme", theme);
        setToggleVisual(theme === "dark");
    }

    function setToggleVisual(on) {
        if (!dom.themeToggle) return;
        if (on) {
            dom.themeToggle.classList.add("on");
            dom.themeToggle.setAttribute("aria-pressed", "true");
        } else {
            dom.themeToggle.classList.remove("on");
            dom.themeToggle.setAttribute("aria-pressed", "false");
        }
    }

    function initTabs() {
        dom.tabs.forEach(tab => {
            tab.addEventListener("click", () => {
                const tabName = tab.getAttribute("data-tab");
                dom.tabs.forEach(t => t.classList.remove("active"));
                tab.classList.add("active");

                Object.entries(dom.tabPanels).forEach(([name, panel]) => {
                    if (name === tabName) {
                        panel.classList.add("active");
                    } else {
                        panel.classList.remove("active");
                    }
                });
            });
        });
    }

    function initEvents() {
        if (dom.themeToggle) {
            dom.themeToggle.addEventListener("click", () => {
                const root = document.documentElement;
                const current = root.getAttribute("data-theme") || "dark";
                const next = current === "dark" ? "light" : "dark";
                root.setAttribute("data-theme", next);
                window.localStorage.setItem("aethericweaver-theme", next);
                setToggleVisual(next === "dark");
            });
        }

        if (dom.configFileInput) {
            dom.configFileInput.addEventListener("change", handleConfigFileSelected);
        }

        if (dom.addSpellBtn) {
            dom.addSpellBtn.addEventListener("click", handleAddSpell);
        }
        if (dom.cloneSpellBtn) {
            dom.cloneSpellBtn.addEventListener("click", handleCloneSpell);
        }
        if (dom.deleteSpellBtn) {
            dom.deleteSpellBtn.addEventListener("click", handleDeleteSpell);
        }

        if (dom.downloadConfigBtn) {
            dom.downloadConfigBtn.addEventListener("click", handleDownloadConfig);
        }

        if (dom.searchInput) {
            dom.searchInput.addEventListener("input", () => renderSpellsTable());
        }
    }

    function handleConfigFileSelected(event) {
        const file = event.target.files && event.target.files[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = e => {
            try {
                const text = e.target.result;
                const json = JSON.parse(text);

                state.spells = Array.isArray(json.Spells) ? json.Spells.map(row => mapRowToSpell(row)) : [];
                state.sets = mapSettingsToSets({ Sets: json.Sets || {} });

                state.selectedSpellIndex = null;
                state.selectedSetIndex = null;
                state.selectedTierIndex = null;

                renderSpellsTable();
                renderSpellDetail();
                renderSetsTable();
                renderSetDetail();
                updateUiState();

                setSpellStatus(`Loaded ${state.spells.length} spells from ${file.name}`);
                setSetStatus(`Loaded ${state.sets.length} equipment set tiers from ${file.name}`);

                if (dom.addSpellBtn) dom.addSpellBtn.disabled = false;
                if (dom.addSetBtn) dom.addSetBtn.disabled = false;
                if (dom.downloadConfigBtn) dom.downloadConfigBtn.disabled = false;
            } catch (err) {
                console.error(err);
                setSpellStatus("Failed to load config JSON. See console for details.");
            }
        };
        reader.readAsText(file);
    }

    function mapRowToSpell(row) {
        return {
            Template: row.Template ?? null,
            Id: row.Id ?? null,
            Name: row.Name ?? null,
            School: row.School ?? null,
            Category: row.Category ?? null,
            Bitfield: row.Bitfield ?? null,
            MetaSpellType: row.MetaSpellType ?? null,
            BaseMana: row.BaseMana ?? null,
            BaseRangeConstant: row.BaseRangeConstant ?? null,
            BaseRangeMod: row.BaseRangeMod ?? null,
            Power: row.Power ?? null,
            StatModType: row.StatModType ?? null,
            StatModKey: row.StatModKey ?? null,
            StatModVal: row.StatModVal ?? null,
            EType: row.EType ?? null,
            DamageType: row.DamageType ?? null,
            BaseIntensity: row.BaseIntensity ?? null,
            Variance: row.Variance ?? null,
            NumProjectiles: row.NumProjectiles ?? null,
            Duration: row.Duration ?? null
        };
    }

    function renderSpellsTable() {
        if (!dom.spellsTableBody) return;
        const filter = (dom.searchInput && dom.searchInput.value || "").toLowerCase();

        dom.spellsTableBody.innerHTML = "";

        state.spells.forEach((spell, index) => {
            const meta = getSpellMetadata(spell);
            if (filter) {
                const haystack = [
                    spell.Template,
                    spell.Id,
                    spell.Name,
                    spell.School,
                    spell.Category,
                    meta && meta.name,
                    meta && meta.type
                ]
                    .filter(x => x != null)
                    .join(" ")
                    .toLowerCase();

                if (!haystack.includes(filter)) {
                    return;
                }
            }

            const tr = document.createElement("tr");
            if (index === state.selectedSpellIndex) {
                tr.classList.add("selected");
            }

            // Icon / type cell
            const iconCell = document.createElement("td");
            iconCell.className = "spell-icon-cell";
            if (meta && meta.iconData) {
                const img = document.createElement("img");
                img.className = "spell-icon-img";
                img.src = meta.iconData;
                img.alt = meta.name || "";
                iconCell.appendChild(img);
            } else if (meta) {
                const pill = document.createElement("div");
                pill.className = "spell-icon-pill";
                if (meta.type) {
                    pill.classList.add(meta.type);
                }
                pill.title = meta.name || "";
                pill.textContent = (meta.iconId != null ? String(meta.iconId) : "?");
                iconCell.appendChild(pill);
            }
            tr.appendChild(iconCell);

            const cells = [
                spell.Template,
                spell.Id,
                spell.Name || (meta && meta.name) || "",
                spell.School,
                spell.Category,
                spell.EType,
                spell.DamageType,
                spell.BaseIntensity,
                spell.StatModVal,
                spell.Duration
            ];

            cells.forEach(value => {
                const td = document.createElement("td");
                td.textContent = value == null ? "" : String(value);
                tr.appendChild(td);
            });

            tr.addEventListener("click", () => {
                state.selectedSpellIndex = index;
                updateUiState();
                renderSpellsTable();
                renderSpellDetail();
            });

            dom.spellsTableBody.appendChild(tr);
        });
    }

    function renderSpellDetail() {
        if (!dom.spellDetail) return;
        const body = dom.spellDetail.querySelector(".panel-body");
        if (!body) return;

        if (state.selectedSpellIndex == null || !state.spells[state.selectedSpellIndex]) {
            body.innerHTML = '<p class="muted">Select a spell to edit its full properties.</p>';
            return;
        }

        const spell = state.spells[state.selectedSpellIndex];
        const meta = getSpellMetadata(spell);

        body.innerHTML = `
            <div class="field-group">
                <label class="field-label">Template <span class="help-icon" data-tooltip="Base spell to copy from; defaults for all unspecified properties come from this spell.">?</span></label>
                <input type="text" data-field="Template" value="${spell.Template ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">Id <span class="help-icon" data-tooltip="Final spell key; if empty, the template id is reused and the existing spell is overridden.">?</span></label>
                <input type="text" data-field="Id" value="${spell.Id ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">Name <span class="help-icon" data-tooltip="Display name shown in-game for this spell.">?</span></label>
                <input type="text" data-field="Name" value="${spell.Name ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">School <span class="help-icon" data-tooltip="Which magic skill is used to cast this spell.">?</span></label>
                <select data-field="School">
                    <option value="">(inherit)</option>
                    <option value="CreatureEnchantment"${spell.School === "CreatureEnchantment" ? " selected" : ""}>Creature</option>
                    <option value="ItemEnchantment"${spell.School === "ItemEnchantment" ? " selected" : ""}>Item</option>
                    <option value="LifeMagic"${spell.School === "LifeMagic" ? " selected" : ""}>Life</option>
                    <option value="WarMagic"${spell.School === "WarMagic" ? " selected" : ""}>War</option>
                    <option value="VoidMagic"${spell.School === "VoidMagic" ? " selected" : ""}>Void</option>
                </select>
            </div>
            <div class="field-group">
                <label class="field-label">Category <span class="help-icon" data-tooltip="Controls stacking; spells with the same category usually overwrite each other.">?</span></label>
                <input type="text" data-field="Category" value="${spell.Category ?? (meta && meta.category) ?? ""}">
            </div>

            <div class="field-group">
                <label class="field-label">BaseMana <span class="help-icon" data-tooltip="Base mana cost before modifiers.">?</span></label>
                <input type="number" data-field="BaseMana" value="${spell.BaseMana ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">Power <span class="help-icon" data-tooltip="Casting difficulty; higher values are harder to cast.">?</span></label>
                <input type="number" data-field="Power" value="${spell.Power ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">StatModVal <span class="help-icon" data-tooltip="How much this spell changes the affected stat (buff, debuff, etc.).">?</span></label>
                <input type="number" step="0.01" data-field="StatModVal" value="${spell.StatModVal ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">Duration <span class="help-icon" data-tooltip="Duration in seconds for buffs and similar effects.">?</span></label>
                <input type="number" step="0.01" data-field="Duration" value="${spell.Duration ?? ""}">
            </div>

            <div class="field-group">
                <label class="field-label">EType <span class="help-icon" data-tooltip="Damage type used for projectiles (fire, acid, etc.).">?</span></label>
                <input type="text" data-field="EType" value="${spell.EType ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">DamageType <span class="help-icon" data-tooltip="Which vital this affects for Life spells (health, mana, stamina).">?</span></label>
                <input type="text" data-field="DamageType" value="${spell.DamageType ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">BaseIntensity <span class="help-icon" data-tooltip="Base damage amount for projectiles.">?</span></label>
                <input type="number" data-field="BaseIntensity" value="${spell.BaseIntensity ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">Variance <span class="help-icon" data-tooltip="Maximum extra random damage added to BaseIntensity.">?</span></label>
                <input type="number" data-field="Variance" value="${spell.Variance ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">NumProjectiles <span class="help-icon" data-tooltip="How many projectiles this spell fires.">?</span></label>
                <input type="number" data-field="NumProjectiles" value="${spell.NumProjectiles ?? ""}">
            </div>

            <div class="advanced-toggle">
                Advanced fields:
                <button type="button" id="toggleAdvancedBtn">Show</button>
            </div>
            <div class="advanced-section" id="advancedSection">
                <div class="field-group">
                    <label class="field-label">Bitfield <span class="help-icon" data-tooltip="Low-level flags that alter spell behavior; usually left alone.">?</span></label>
                    <input type="text" data-field="Bitfield" value="${spell.Bitfield ?? ""}">
                </div>
                <div class="field-group">
                    <label class="field-label">MetaSpellType <span class="help-icon" data-tooltip="Sub-type classification used by the core spell system.">?</span></label>
                    <input type="text" data-field="MetaSpellType" value="${spell.MetaSpellType ?? ""}">
                </div>
                <div class="field-group">
                    <label class="field-label">BaseRangeConstant <span class="help-icon" data-tooltip="Base maximum range before skill-based modifiers.">?</span></label>
                    <input type="number" step="0.01" data-field="BaseRangeConstant" value="${spell.BaseRangeConstant ?? ""}">
                </div>
                <div class="field-group">
                    <label class="field-label">BaseRangeMod <span class="help-icon" data-tooltip="How much skill increases the spell range.">?</span></label>
                    <input type="number" step="0.01" data-field="BaseRangeMod" value="${spell.BaseRangeMod ?? ""}">
                </div>
                <div class="field-group">
                    <label class="field-label">CasterEffect <span class="help-icon" data-tooltip="Visual effect that plays on the caster.">?</span></label>
                    <input type="text" data-field="CasterEffect" value="${spell.CasterEffect ?? ""}">
                </div>
                <div class="field-group">
                    <label class="field-label">TargetEffect <span class="help-icon" data-tooltip="Visual effect that plays on the target.">?</span></label>
                    <input type="text" data-field="TargetEffect" value="${spell.TargetEffect ?? ""}">
                </div>
                <div class="field-group">
                    <label class="field-label">NonComponentTargetType <span class="help-icon" data-tooltip="Allowed non-component target types; rarely changed.">?</span></label>
                    <input type="text" data-field="NonComponentTargetType" value="${spell.NonComponentTargetType ?? ""}">
                </div>
                <div class="field-group">
                    <label class="field-label">Wcid <span class="help-icon" data-tooltip="Projectile weenie class id; must be a SpellProjectile or the server can crash.">?</span></label>
                    <input type="number" data-field="Wcid" value="${spell.Wcid ?? ""}">
                </div>
            </div>
            <p class="muted">Core fields are shown above. Open advanced fields only when you need fine-grained control.</p>
        `;

        const advancedSection = body.querySelector("#advancedSection");
        const toggleBtn = body.querySelector("#toggleAdvancedBtn");
        if (toggleBtn && advancedSection) {
            toggleBtn.addEventListener("click", () => {
                const visible = advancedSection.classList.toggle("visible");
                toggleBtn.textContent = visible ? "Hide" : "Show";
            });
        }

        body.querySelectorAll("input, select").forEach(input => {
            input.addEventListener("input", () => {
                const field = input.getAttribute("data-field");
                if (!field) return;
                const raw = input.value;
                const value = raw === "" ? null : raw;
                state.spells[state.selectedSpellIndex][field] = value;
                renderSpellsTable();
            });
        });
    }

    function getSpellMetadata(spell) {
        if (!spell) return null;
        const byEnum = state.metadata.spellsByEnum;
        const byId = state.metadata.spellsById;

        if (spell.Template && byEnum[String(spell.Template)]) {
            return byEnum[String(spell.Template)];
        }
        if (spell.Id != null && byEnum[String(spell.Id)]) {
            return byEnum[String(spell.Id)];
        }
        if (spell.Id != null && byId[String(spell.Id)]) {
            return byId[String(spell.Id)];
        }
        return null;
    }

    function mapSettingsToSets(json) {
        const sets = [];
        if (!json || !json.Sets) {
            return sets;
        }

        Object.entries(json.Sets).forEach(([key, tiers]) => {
            tiers.forEach(tier => {
                sets.push({
                    EquipmentSet: key,
                    NumEquipped: tier.NumEquipped,
                    Spells: tier.Spells || []
                });
            });
        });

        return sets;
    }

    function mapSetsToSettings(sets) {
        const result = {};
        for (const entry of sets) {
            const key = entry.EquipmentSet ?? "";
            if (!key) continue;
            if (!result[key]) {
                result[key] = [];
            }
            result[key].push({
                NumEquipped: entry.NumEquipped,
                Spells: entry.Spells || []
            });
        }
        return { Sets: result };
    }

    function renderSetsTable() {
        if (!dom.setsTableBody) return;
        dom.setsTableBody.innerHTML = "";

        state.sets.forEach((entry, index) => {
            const tr = document.createElement("tr");
            if (index === state.selectedSetIndex) {
                tr.classList.add("selected");
            }

            const tdSet = document.createElement("td");
            tdSet.textContent = entry.EquipmentSet;
            tr.appendChild(tdSet);

            const tdTier = document.createElement("td");
            tdTier.textContent = entry.NumEquipped;
            tr.appendChild(tdTier);

            const tdSpells = document.createElement("td");
            const labels = (entry.Spells || []).map(id => {
                const meta = state.metadata.spellsByEnum[String(id)] || state.metadata.spellsById[String(id)];
                if (meta && meta.name) {
                    return `${id} (${meta.name})`;
                }
                return String(id);
            });
            tdSpells.textContent = labels.join(", ");
            tr.appendChild(tdSpells);

            tr.addEventListener("click", () => {
                state.selectedSetIndex = index;
                renderSetsTable();
                renderSetDetail();
            });

            dom.setsTableBody.appendChild(tr);
        });
    }

    function renderSetDetail() {
        if (!dom.setDetail) return;
        const body = dom.setDetail.querySelector(".panel-body");
        if (!body) return;

        if (state.selectedSetIndex == null || !state.sets[state.selectedSetIndex]) {
            body.innerHTML = '<p class="muted">Select a set/tier to edit its spells.</p>';
            return;
        }

        const entry = state.sets[state.selectedSetIndex];

        body.innerHTML = `
            <div class="field-group">
                <label class="field-label">EquipmentSet</label>
                <input type="text" data-field="EquipmentSet" value="${entry.EquipmentSet ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">NumEquipped</label>
                <input type="number" data-field="NumEquipped" value="${entry.NumEquipped ?? ""}">
            </div>
            <div class="field-group">
                <label class="field-label">Spells (comma-separated SpellId or numeric ids)</label>
                <input type="text" data-field="Spells" value="${(entry.Spells || []).join(", ")}">
            </div>
            <p class="muted">This interface mirrors the <code>Sets</code> / <code>SetTier</code> structure from Settings.json. A richer tier editor can be layered on later.</p>
        `;

        body.querySelectorAll("input").forEach(input => {
            input.addEventListener("input", () => {
                const field = input.getAttribute("data-field");
                if (!field) return;
                const value = input.value;

                if (field === "Spells") {
                    state.sets[state.selectedSetIndex].Spells = value
                        .split(",")
                        .map(x => x.trim())
                        .filter(x => x.length > 0);
                } else if (field === "NumEquipped") {
                    const n = value === "" ? null : Number(value);
                    state.sets[state.selectedSetIndex].NumEquipped = Number.isNaN(n) ? null : n;
                } else {
                    state.sets[state.selectedSetIndex][field] = value === "" ? null : value;
                }

                renderSetsTable();
            });
        });
    }

    function handleAddSpell() {
        const spell = {
            Template: null,
            Id: null,
            Name: "",
            School: null,
            Category: null
        };
        state.spells.push(spell);
        state.selectedSpellIndex = state.spells.length - 1;
        renderSpellsTable();
        renderSpellDetail();
        updateUiState();
        setSpellStatus("Added new spell customization.");
    }

    function handleCloneSpell() {
        if (state.selectedSpellIndex == null || !state.spells[state.selectedSpellIndex]) return;
        const original = state.spells[state.selectedSpellIndex];
        const clone = { ...original, Id: null };
        state.spells.push(clone);
        state.selectedSpellIndex = state.spells.length - 1;
        renderSpellsTable();
        renderSpellDetail();
        updateUiState();
        setSpellStatus("Cloned selected spell. Id cleared so you can assign a new key.");
    }

    function handleDeleteSpell() {
        if (state.selectedSpellIndex == null) return;
        const index = state.selectedSpellIndex;
        state.spells.splice(index, 1);
        state.selectedSpellIndex = null;
        renderSpellsTable();
        renderSpellDetail();
        updateUiState();
        setSpellStatus("Deleted selected spell.");
    }

    function handleDownloadConfig() {
        const settingsLike = mapSetsToSettings(state.sets);
        const payload = {
            Spells: state.spells,
            Sets: settingsLike.Sets
        };

        const json = JSON.stringify(payload, null, 2);
        const blob = new Blob([json], { type: "application/json" });
        const url = URL.createObjectURL(blob);

        const a = document.createElement("a");
        a.href = url;
        a.download = "AethericWeaverConfig.json";
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }

    function setSpellStatus(message) {
        if (!dom.spellStatus) return;
        dom.spellStatus.textContent = message || "";
    }

    function setSetStatus(message) {
        if (!dom.setStatus) return;
        dom.setStatus.textContent = message || "";
    }

    function updateUiState() {
        const hasSpells = state.spells.length > 0;
        const spellSelected = state.selectedSpellIndex != null;

        if (dom.cloneSpellBtn) dom.cloneSpellBtn.disabled = !spellSelected;
        if (dom.deleteSpellBtn) dom.deleteSpellBtn.disabled = !spellSelected;

        const hasSets = state.sets.length > 0;
        const setSelected = state.selectedSetIndex != null;

        if (dom.addTierBtn) dom.addTierBtn.disabled = !hasSets;
        if (dom.deleteTierBtn) dom.deleteTierBtn.disabled = !setSelected;
    }
})();

