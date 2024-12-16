$(document).ready(function () {
    $('#hoaTable').DataTable({
        ajax: {
            url: '/Admin/Hoa/List',
            type: 'GET',
            dataSrc: 'data'
        },
        columns: [
            {
                data: null,
                render: (data, type, row, meta) => meta.row + 1,
                width: "10%"
            }, // Số thứ tự
            {
                data: 'name',
                width: "20%"
            },
            {
                data: 'description',
                width: "20%"
            },
            {
                data: 'price',
                render: (data) => {
                    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(data);
                },
                width: "10%"
            },
            {
                data: 'stockQuantity',
                width: "10%"
            },
            {
                data: 'loaiHoaName',
                width: "10%"
            },
            {
                data: 'id',
                render: (data, type, row) => {
                    return `
                            <a href="/Admin/Hoa/Details/${data}" class="btn btn-dark btn-sm">Xem</a>
                            <a href="/Admin/Hoa/Edit/${data}" class="btn btn-success btn-sm">Sửa</a>
                            <a onClick="confirmDelete('/Admin/Hoa/Delete/${data}')" class="btn btn-danger btn-sm">Xóa</a>
                        `;
                },
                width: "20%"
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

function confirmDelete(url) {
    Swal.fire({
        title: 'Bạn có chắc chắn muốn xóa?',
        text: "Bạn sẽ không thể khôi phục lại dữ liệu này!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Xóa!',
        cancelButtonText: 'Hủy bỏ'
    }).then((result) => {
        if (result.isConfirmed) {
            deleteHoa(url);
        }
    });
}

function deleteHoa(url) {
    var currentPage = $('#hoaTable').DataTable().page();

    $.ajax({
        url: url,
        type: "POST",
        success: function (response) {
            if (response.success) {
                Swal.fire(
                    'Đã xóa!',
                    'Dữ liệu của bạn đã được xóa.',
                    'success'
                );
                var table = $('#hoaTable').DataTable();
                table.ajax.reload(function () {
                    table.page(currentPage).draw('page');
                });
            } else {
                Swal.fire(
                    'Lỗi!',
                    response.message,
                    'error'
                );
            }
        },
        error: function (xhr, status, error) {
            Swal.fire(
                'Lỗi!',
                'Đã xảy ra lỗi: ' + error,
                'error'
            );
        }
    });
}