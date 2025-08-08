function generatePieChart(canvasId, name, labels, data, total) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) return;

    // Destroy old chart
    if (canvas._chartInstance) {
        canvas._chartInstance.destroy();
    }

    // Generate dynamic colors
    const colors = labels.map((_, i) => {
        const hue = (i * 360 / labels.length + 20) % 360;
        const saturation = 60 + Math.floor(Math.random() * 10); // 60-70%
        const lightness = 50 + Math.floor(Math.random() * 10);  // 50-60%
        return `hsl(${hue}, ${saturation}%, ${lightness}%)`;
    });

    const chart = new Chart(canvas, {
        type: "pie",
        data: {
            labels,
            datasets: [
                {
                    data,
                    backgroundColor: colors,
                },
            ],
        },
        options: {
            responsive: false,
            plugins: {
                title: {
                    display: true,
                    text: name,
                    font: {
                        size: 18
                    },
                    padding: {
                        top: 10,
                        bottom: 20
                    }
                },
                legend: {
                    display: false,
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const value = context.raw;
                            const label = context.label || '';
                            let percentage = ((value / total) * 100).toFixed(1);
                            return `${label}: ₹${value.toFixed(2)} (${percentage}%)`;
                        },
                    },
                },
                datalabels: {
                    display: true,
                    align: "start",
                    anchor: "end",
                    offset: 10, 
                    color: '#000',
                    formatter: function (value, context) {
                        const category = context.chart.data.labels[context.dataIndex];
                        let percentage = ((value / total) * 100).toFixed(1) + '%';
                        return `${percentage}`;
                    },
                    font: {
                        weight: 'bold',
                        size: 12
                    }
                }
            },
        },
    });

    // Save instanc
    canvas._chartInstance = chart;

    // Create legends
    const legendDiv = canvas.nextElementSibling;
    if (legendDiv) {
        legendDiv.innerHTML = labels
            .map((label, i) => `<div class="legend-item"><span class="legend-color" style="background:${colors[i]}"></span><span class="legend-label"><strong>${label}:</strong> ₹${data[i].toFixed(2)} (${((data[i]  / total) * 100).toFixed(1)}%)</span></div>`).join("");
    }
}
