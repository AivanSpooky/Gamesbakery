class StatusMonitor {
    constructor() {
        this.autoRefreshInterval = null;
        this.currentRefreshRate = 10000; // 10 seconds default
        this.init();
    }

    init() {
        this.bindEvents();
        this.loadStatus();
        this.startAutoRefresh();
    }

    bindEvents() {
        // Auto-refresh dropdown
        const autoRefreshSelect = document.getElementById('autoRefresh');
        autoRefreshSelect.addEventListener('change', (e) => {
            this.currentRefreshRate = parseInt(e.target.value);
            this.startAutoRefresh();
        });

        // Raw data toggle
        const toggleBtn = document.getElementById('toggleRawData');
        const rawDataContent = document.getElementById('rawDataContent');
        
        toggleBtn.addEventListener('click', () => {
            const isExpanded = rawDataContent.classList.contains('expanded');
            if (isExpanded) {
                rawDataContent.classList.remove('expanded');
                toggleBtn.innerHTML = '<i class="fas fa-chevron-down"></i>';
            } else {
                rawDataContent.classList.add('expanded');
                toggleBtn.innerHTML = '<i class="fas fa-chevron-up"></i>';
            }
        });

        // Manual refresh on card click
        document.querySelectorAll('.status-card').forEach(card => {
            card.addEventListener('click', () => {
                this.loadStatus();
            });
        });
    }

    async loadStatus() {
        this.showLoading();
        
        try {
            const response = await fetch('/nginx_status');
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            
            const data = await response.text();
            this.parseStatusData(data);
            this.updateLastUpdateTime();
            this.checkServices();
        } catch (error) {
            console.error('Error loading status:', error);
            this.showError(error.message);
        } finally {
            this.hideLoading();
        }
    }

    parseStatusData(rawData) {
        // Update raw data display
        document.getElementById('rawStatusData').textContent = rawData;

        // Parse NGINX status data
        const lines = rawData.split('\n').filter(line => line.trim());
        
        // Active connections
        const activeMatch = lines[0].match(/Active connections:\s+(\d+)/);
        if (activeMatch) {
            document.getElementById('activeConnections').textContent = activeMatch[1];
        }

        // Server accepts, handled, requests
        const serverMatch = lines[2].match(/\s+(\d+)\s+(\d+)\s+(\d+)/);
        if (serverMatch) {
            document.getElementById('accepted').textContent = this.formatNumber(serverMatch[1]);
            document.getElementById('handled').textContent = this.formatNumber(serverMatch[2]);
            document.getElementById('requests').textContent = this.formatNumber(serverMatch[3]);
            
            // Calculate efficiency
            // const accepted = parseInt(serverMatch[1]);
            // const handled = parseInt(serverMatch[2]);
            // const efficiency = accepted > 0 ? ((handled / accepted) * 100).toFixed(2) : '100.00';
            // document.getElementById('efficiency').textContent = `${efficiency}%`;
        }

        // Reading, Writing, Waiting
        const connMatch = lines[3].match(/Reading:\s+(\d+)\s+Writing:\s+(\d+)\s+Waiting:\s+(\d+)/);
        if (connMatch) {
            document.getElementById('reading').textContent = connMatch[1];
            document.getElementById('writing').textContent = connMatch[2];
            document.getElementById('waiting').textContent = connMatch[3];
        }
    }

    async checkServices() {
        // Check database status
        this.updateServiceStatus('dbStatus', 'checking');
        
        // Check web application status
        this.updateServiceStatus('webStatus', 'checking');
        
        // Check adminer status
        this.updateServiceStatus('adminerStatus', 'checking');
        
        // Check nginx status (self)
        this.updateServiceStatus('nginxStatus', 'healthy');

        // Simulate service checks (in real implementation, these would be actual API calls)
        setTimeout(() => {
            this.updateServiceStatus('dbStatus', 'healthy');
        }, 1000);

        setTimeout(() => {
            this.updateServiceStatus('webStatus', 'healthy');
        }, 1500);

        setTimeout(() => {
            this.updateServiceStatus('adminerStatus', 'healthy');
        }, 2000);
    }

    updateServiceStatus(elementId, status) {
        const element = document.getElementById(elementId);
        const dot = element.querySelector('.status-dot');
        
        // Remove existing status classes
        dot.classList.remove('healthy', 'warning', 'error');
        
        switch (status) {
            case 'healthy':
                dot.classList.add('healthy');
                element.innerHTML = '<span class="status-dot healthy"></span> Работает';
                break;
            case 'warning':
                dot.classList.add('warning');
                element.innerHTML = '<span class="status-dot warning"></span> Предупреждение';
                break;
            case 'error':
                dot.classList.add('error');
                element.innerHTML = '<span class="status-dot error"></span> Ошибка';
                break;
            case 'checking':
                dot.classList.add('warning');
                element.innerHTML = '<span class="status-dot warning"></span> Проверка...';
                break;
        }
    }

    updateLastUpdateTime() {
        const now = new Date();
        const timeString = now.toLocaleTimeString('ru-RU');
        const dateString = now.toLocaleDateString('ru-RU');
        document.getElementById('lastUpdate').textContent = 
            `Последнее обновление: ${dateString} ${timeString}`;
    }

    startAutoRefresh() {
        // Clear existing interval
        if (this.autoRefreshInterval) {
            clearInterval(this.autoRefreshInterval);
        }

        // Start new interval if not disabled
        if (this.currentRefreshRate > 0) {
            this.autoRefreshInterval = setInterval(() => {
                this.loadStatus();
            }, this.currentRefreshRate);
        }
    }

    showLoading() {
        document.getElementById('loadingOverlay').classList.add('active');
        
        // Add updating animation to cards
        document.querySelectorAll('.status-card').forEach(card => {
            card.classList.add('updating');
        });
    }

    hideLoading() {
        document.getElementById('loadingOverlay').classList.remove('active');
        
        // Remove updating animation
        document.querySelectorAll('.status-card').forEach(card => {
            card.classList.remove('updating');
        });
    }

    showError(message) {
        // Update raw data with error
        document.getElementById('rawStatusData').textContent = `Ошибка загрузки статуса: ${message}`;
        
        // Show error in status indicators
        this.updateServiceStatus('nginxStatus', 'error');
        
        // Update last update time with error
        const now = new Date();
        const timeString = now.toLocaleTimeString('ru-RU');
        document.getElementById('lastUpdate').textContent = 
            `Ошибка обновления: ${timeString}`;
    }

    formatNumber(num) {
        return parseInt(num).toLocaleString('ru-RU');
    }
}

// Initialize the status monitor when the page loads
document.addEventListener('DOMContentLoaded', () => {
    new StatusMonitor();
});

// Add some visual effects
document.addEventListener('DOMContentLoaded', () => {
    // Animate cards on load
    const cards = document.querySelectorAll('.status-card');
    cards.forEach((card, index) => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        
        setTimeout(() => {
            card.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
            card.style.opacity = '1';
            card.style.transform = 'translateY(0)';
        }, index * 100);
    });
});