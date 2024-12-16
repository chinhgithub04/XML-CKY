$(document).ready(function () {
    $('#orderTable').DataTable({
        ajax: {
            url: '/Admin/OrderManagement/List',
            type: 'GET',
            dataSrc: function (json) {
                // Filter orders to only include those with status 1 or 2
                return json.data.filter(order => order.status === 1 || order.status === 2);
            }
        },
        columns: [
            {
                data: 'orderId',
                width: "17%"
            },
            {
                data: 'userId',
                width: "15%"
            },
            {
                data: 'name',
                width: "20%"
            },
            {
                data: 'hoaName',
                width: "20%"
            },
            {
                data: 'status', title: 'Trạng thái', width: "12%", render: function (data) {
                    switch (data) {
                        case 1: return '<span class="badge bg-warning text-dark">Đang giao</span>';
                        case 2: return '<span class="badge bg-success">Đã giao</span>';
                        default: return 'Không xác định';
                    }
                }
            },
            {
                data: 'orderId',
                render: (data, type, row) => {
                    let editButton = row.status === 2 ? '' : `<a href="/Admin/OrderManagement/Edit/${data}" class="btn btn-success btn-sm">Sửa</a>`;
                    return `
                        <a href="/Admin/OrderManagement/Details/${data}" class="btn btn-dark btn-sm">Xem</a>
                        ${editButton}
                    `;
                },
                width: "16%"
            }
        ],
        language: {
            paginate: {
                next: 'Next →',
                previous: '← Prev'
            }
        }
    });
});