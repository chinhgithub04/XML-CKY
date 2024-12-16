$(document).ready(function () {
    $('#orderTable').DataTable({
        ajax: {
            url: '/Client/Account/List',
            type: 'GET',
            dataSrc: 'data'
        },
        columns: [
            { data: 'id', title: 'Mã hàng', width: "25%" },
            { data: 'hoaName', title: 'Tên hoa', width: "22%" },
            { data: 'quantity', title: 'Số lượng', width: "10%" },
            {
                data: 'orderDate', title: 'Ngày đặt', width: "12%", render: function (data) {
                    return new Date(data).toLocaleDateString('vi-VN');
                }
            },
            {
                data: 'totalPrice', title: 'Tổng tiền', width: "15%", render: function (data) {
                    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(data);
                }
            },
            {
                data: 'status', title: 'Trạng thái', width: "13%", render: function (data) {
                    switch (data) {
                        case 1: return '<span class="badge bg-warning text-dark">Đang giao</span>';
                        case 2: return '<span class="badge bg-success">Đã giao</span>';
                        case 3: return '<span class="badge bg-danger">Đã hủy</span>';
                        default: return 'Không xác định';
                    }
                }
            },
            {
                data: 'id', title: 'Thao tác', width: "10%", render: function (data, type, row) {
                    if (row.status === 1) {
                        return `<button class="btn btn-danger btn-sm" onclick="cancelOrder('${data}')">Hủy</button>`;
                    }
                    return '';
                }
            }
        ]
    });
});

function cancelOrder(orderId) {
    Swal.fire({
        title: 'Bạn có chắc chắn muốn hủy đơn hàng này không?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Hủy đơn hàng',
        cancelButtonText: 'Không'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: '/Client/Account/CancelOrder',
                type: 'POST',
                contentType: 'application/json',
                data: JSON.stringify(orderId),
                success: function () {
                    $('#orderTable').DataTable().ajax.reload();
                    Swal.fire('Đơn hàng đã được hủy thành công.', '', 'success');
                },
                error: function () {
                    Swal.fire('Đã xảy ra lỗi khi hủy đơn hàng.', '', 'error');
                }
            });
        }
    });
}