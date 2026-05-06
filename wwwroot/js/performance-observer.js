/**
 * Performance Observer - Core Web Vitals Collection and Dashboard
 * Collects LCP, CLS, FID metrics via PerformanceObserver API
 * and reports them to the server. Renders Chart.js visualizations.
 */
(function () {
    'use strict';

    const budgets = JSON.parse(document.getElementById('budget-data').textContent);
    const charts = {};
    const pageUrl = window.location.pathname;

    // Initialize charts for each metric
    function initCharts() {
        budgets.forEach(function (budget) {
            const canvas = document.getElementById('chart-' + budget.metricName);
            if (!canvas) return;

            const ctx = canvas.getContext('2d');
            charts[budget.metricName] = new Chart(ctx, {
                type: 'line',
                data: {
                    labels: [],
                    datasets: [{
                        label: budget.metricName + (budget.unit ? ' (' + budget.unit + ')' : ''),
                        data: [],
                        borderColor: '#0d6efd',
                        backgroundColor: 'rgba(13, 110, 253, 0.1)',
                        fill: true,
                        tension: 0.3
                    }]
                },
                options: {
                    responsive: true,
                    plugins: {
                        annotation: {
                            annotations: {
                                warningLine: {
                                    type: 'line',
                                    yMin: budget.warningThreshold,
                                    yMax: budget.warningThreshold,
                                    borderColor: '#ffc107',
                                    borderWidth: 2,
                                    borderDash: [6, 4],
                                    label: {
                                        display: true,
                                        content: 'Warning: ' + budget.warningThreshold + (budget.unit || ''),
                                        position: 'start',
                                        backgroundColor: '#ffc107',
                                        color: '#000'
                                    }
                                },
                                errorLine: {
                                    type: 'line',
                                    yMin: budget.errorThreshold,
                                    yMax: budget.errorThreshold,
                                    borderColor: '#dc3545',
                                    borderWidth: 2,
                                    borderDash: [6, 4],
                                    label: {
                                        display: true,
                                        content: 'Error: ' + budget.errorThreshold + (budget.unit || ''),
                                        position: 'start',
                                        backgroundColor: '#dc3545',
                                        color: '#fff'
                                    }
                                }
                            }
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true,
                            title: {
                                display: true,
                                text: budget.unit || 'score'
                            }
                        },
                        x: {
                            title: {
                                display: true,
                                text: 'Time'
                            }
                        }
                    }
                }
            });
        });
    }

    // Report a metric to the server
    function reportMetric(name, value, unit) {
        const payload = {
            metricName: name,
            value: value,
            unit: unit || 'ms',
            pageUrl: pageUrl,
            timestamp: new Date().toISOString()
        };

        fetch('/Performance/Report', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        }).catch(function (err) {
            console.warn('Failed to report metric:', err);
        });
    }

    // Update traffic-light indicator
    function updateStatus(metricName, value) {
        const budget = budgets.find(function (b) { return b.metricName === metricName; });
        if (!budget) return;

        const indicator = document.getElementById('status-' + metricName);
        const valueDisplay = document.getElementById('value-' + metricName);

        if (!indicator || !valueDisplay) return;

        valueDisplay.textContent = value.toFixed(budget.unit === 'ms' ? 0 : 3) + (budget.unit || '');

        let color;
        if (value <= budget.warningThreshold) {
            color = '#198754'; // green
        } else if (value <= budget.errorThreshold) {
            color = '#ffc107'; // yellow
        } else {
            color = '#dc3545'; // red
        }
        indicator.style.backgroundColor = color;

        updateSuggestions();
    }

    // Update suggestions panel
    function updateSuggestions() {
        const panel = document.getElementById('suggestions-panel');
        if (!panel) return;

        const suggestions = [];

        budgets.forEach(function (budget) {
            const valueEl = document.getElementById('value-' + budget.metricName);
            if (!valueEl || valueEl.textContent === '--') return;

            const value = parseFloat(valueEl.textContent);
            if (isNaN(value)) return;

            if (value > budget.errorThreshold) {
                suggestions.push(getSuggestion(budget.metricName, 'error'));
            } else if (value > budget.warningThreshold) {
                suggestions.push(getSuggestion(budget.metricName, 'warning'));
            }
        });

        if (suggestions.length === 0) {
            panel.innerHTML = '<p class="text-success"><strong>All metrics within budget!</strong> Great performance.</p>';
        } else {
            panel.innerHTML = '<ul class="list-group">' +
                suggestions.map(function (s) {
                    return '<li class="list-group-item list-group-item-' + s.level + '">' + s.text + '</li>';
                }).join('') + '</ul>';
        }
    }

    // Get suggestion text for a metric
    function getSuggestion(metricName, level) {
        var levelClass = level === 'error' ? 'danger' : 'warning';
        var suggestions = {
            'LCP': 'Largest Contentful Paint is high. Consider optimizing images, preloading critical resources, or using a CDN.',
            'CLS': 'Cumulative Layout Shift is high. Set explicit dimensions on images/videos and avoid injecting content above the fold.',
            'TTFB': 'Time to First Byte is high. Consider server-side caching, CDN usage, or optimizing database queries.',
            'FID': 'First Input Delay is high. Reduce JavaScript execution time, break up long tasks, and use web workers.'
        };
        return { text: suggestions[metricName] || metricName + ' exceeds budget.', level: levelClass };
    }

    // Fetch history and update charts
    function fetchHistory() {
        budgets.forEach(function (budget) {
            fetch('/Performance/History?metricName=' + encodeURIComponent(budget.metricName) + '&minutes=60')
                .then(function (r) { return r.json(); })
                .then(function (data) {
                    var chart = charts[budget.metricName];
                    if (!chart || !data.length) return;

                    chart.data.labels = data.map(function (d) {
                        return new Date(d.timestamp).toLocaleTimeString();
                    });
                    chart.data.datasets[0].data = data.map(function (d) { return d.value; });
                    chart.update();

                    // Update status with latest value
                    var latest = data[data.length - 1];
                    updateStatus(budget.metricName, latest.value);
                })
                .catch(function (err) {
                    console.warn('Failed to fetch history for ' + budget.metricName + ':', err);
                });
        });
    }

    // Observe Core Web Vitals using PerformanceObserver
    function observeWebVitals() {
        if (!('PerformanceObserver' in window)) return;

        // Observe LCP
        try {
            var lcpObserver = new PerformanceObserver(function (list) {
                var entries = list.getEntries();
                var lastEntry = entries[entries.length - 1];
                if (lastEntry) {
                    reportMetric('LCP', lastEntry.startTime, 'ms');
                    updateStatus('LCP', lastEntry.startTime);
                }
            });
            lcpObserver.observe({ type: 'largest-contentful-paint', buffered: true });
        } catch (e) { /* LCP not supported */ }

        // Observe CLS
        try {
            var clsValue = 0;
            var clsObserver = new PerformanceObserver(function (list) {
                list.getEntries().forEach(function (entry) {
                    if (!entry.hadRecentInput) {
                        clsValue += entry.value;
                    }
                });
                reportMetric('CLS', clsValue, '');
                updateStatus('CLS', clsValue);
            });
            clsObserver.observe({ type: 'layout-shift', buffered: true });
        } catch (e) { /* CLS not supported */ }

        // Observe FID
        try {
            var fidObserver = new PerformanceObserver(function (list) {
                var entries = list.getEntries();
                var firstEntry = entries[0];
                if (firstEntry) {
                    reportMetric('FID', firstEntry.processingStart - firstEntry.startTime, 'ms');
                    updateStatus('FID', firstEntry.processingStart - firstEntry.startTime);
                }
            });
            fidObserver.observe({ type: 'first-input', buffered: true });
        } catch (e) { /* FID not supported */ }
    }

    // Collect TTFB from Navigation Timing API
    function collectTTFB() {
        try {
            var navEntries = performance.getEntriesByType('navigation');
            if (navEntries && navEntries.length > 0) {
                var navEntry = navEntries[0];
                var ttfb = navEntry.responseStart - navEntry.requestStart;
                if (ttfb >= 0) {
                    reportMetric('TTFB', ttfb, 'ms');
                    updateStatus('TTFB', ttfb);
                }
            }
        } catch (e) { /* Navigation Timing not supported */ }
    }

    // Initialize on DOM ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function init() {
        initCharts();
        observeWebVitals();
        // Collect TTFB after current synchronous stack clears so navigation entry is finalized
        setTimeout(collectTTFB, 0);
        fetchHistory();
        // Refresh chart data periodically
        setInterval(fetchHistory, 30000);
    }
})();
