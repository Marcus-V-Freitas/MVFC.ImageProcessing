const STORAGE_URL = 'http://localhost:4443/storage/v1/b';
const API_URL = 'http://localhost:8081'; // Upload API

async function fetchState() {
    try {
        const response = await fetch('/api/files');
        const state = await response.json();
        renderGallery(state);
    } catch (e) {
        console.error("Failed to fetch state", e);
    }
}

async function fetchAnalysisJson(filename) {
    try {
        const res = await fetch(`${STORAGE_URL}/analysis-results/o/${filename}?alt=media`);
        if(res.ok) return await res.json();
    } catch {}
    return null;
}

async function renderGallery(state) {
    const gallery = document.getElementById('gallery');
    gallery.innerHTML = '';

    // Reverse to show newest first
    for (const uploadName of [...state.uploads].reverse()) {
        const origUrl = `${STORAGE_URL}/uploads/o/${uploadName}?alt=media`;
        
        // Thumbnail is always converted to PNG by the backend worker
        const baseName = uploadName.substring(0, uploadName.lastIndexOf('.')) || uploadName;
        const thumbName = `thumb-${baseName}.png`;
        const hasThumb = state.thumbnails.includes(thumbName);
        const thumbUrl = hasThumb ? `${STORAGE_URL}/thumbnails/o/${thumbName}?alt=media` : null;

        const analysisName = `analysis-${uploadName}.json`;
        const hasAnalysis = state.analyses.includes(analysisName);
        
        let analysisHtml = `<div class="pending"><div class="spinner"></div>Processing image...</div>`;
        
        if (hasAnalysis) {
            const data = await fetchAnalysisJson(analysisName);
            if (data) {
                analysisHtml = `
                    <div class="analysis-data">
                        <p style="line-height: 1.5;">${data.description}</p>
                        
                        <div class="color-dots">
                            ${(data.dominant_colors || []).map(c => `<div class="color-dot" style="background-color: ${c}" title="${c}"></div>`).join('')}
                        </div>
                    </div>
                `;
            }
        }

        const card = document.createElement('div');
        card.className = 'card';
        card.innerHTML = `
            <div class="card-header">
                ${uploadName}
                <button class="delete-btn" onclick="deleteFile('${uploadName}')" title="Delete Image">🗑️</button>
            </div>
            <div class="images-container">
                <div class="image-box" data-label="Original">
                    <img src="${origUrl}" alt="Original">
                </div>
                <div class="image-box" data-label="Thumbnail">
                    ${hasThumb ? `<img src="${thumbUrl}" alt="Thumbnail">` : `<div style="display:flex;height:100%;align-items:center;justify-content:center;color:#666;">Waiting...</div>`}
                </div>
            </div>
            ${analysisHtml}
        `;
        gallery.appendChild(card);
    }
}

// Upload Handling
const fileInput = document.getElementById('fileInput');
const dropZone = document.getElementById('dropZone');

fileInput.addEventListener('change', handleFiles);

dropZone.addEventListener('dragover', (e) => {
    e.preventDefault();
    dropZone.classList.add('dragover');
});

dropZone.addEventListener('dragleave', () => {
    dropZone.classList.remove('dragover');
});

dropZone.addEventListener('drop', (e) => {
    e.preventDefault();
    dropZone.classList.remove('dragover');
    if (e.dataTransfer.files.length) {
        fileInput.files = e.dataTransfer.files;
        handleFiles();
    }
});

async function handleFiles() {
    if (!fileInput.files.length) return;
    
    const file = fileInput.files[0];
    const formData = new FormData();
    formData.append('file', file);

    dropZone.style.opacity = '0.5';
    
    try {
        await fetch(`${API_URL}/upload`, {
            method: 'POST',
            body: formData
        });
        
        // Immediately fetch state after successful upload
        setTimeout(fetchState, 500);
    } catch (e) {
        alert('Upload failed: ' + e.message);
    } finally {
        dropZone.style.opacity = '1';
        fileInput.value = '';
    }
}

async function deleteFile(fileName) {
    if(!confirm(`Delete ${fileName}?`)) return;
    try {
        await fetch(`/api/delete/${fileName}`, { method: 'POST' });
        setTimeout(fetchState, 500);
    } catch(e) {
        alert('Delete failed: ' + e.message);
    }
}

// Server-Sent Events (SSE) — real-time updates from Pub/Sub
const evtSource = new EventSource('/events/stream');

evtSource.addEventListener('gallery-updated', () => {
    fetchState();
});

evtSource.onerror = () => {
    console.warn('SSE connection lost, will auto-reconnect...');
};

// Fallback poll every 30s as safety net
setInterval(fetchState, 30000);
fetchState();

