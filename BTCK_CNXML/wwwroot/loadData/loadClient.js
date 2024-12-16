$(document).ready(function () {
    $('#clientTable').DataTable({
        ajax: {
            url: '/Admin/Client/List',
            type: 'GET',
            dataSrc: 'data'
        },
        columns: [
            {
                data: 'id',
                width: "20%"
            },
            {
                data: 'name',
                width: "20%"
            },
            {
                data: 'email',
                width: "20%"
            },
            {
                data: 'phoneNumber',
                width: "15%"
            },
            {
                data: 'id',
                render: (data, type, row) => {
                    return `
                            <a href="/Admin/Client/Details/${data}" class="btn btn-dark btn-sm">Xem</a>
                            <a href="/Admin/Client/Edit/${data}" class="btn btn-success btn-sm">Sửa</a>
                            <a onClick="confirmDelete('/Admin/Client/Delete/${data}')" class="btn btn-danger btn-sm">Xóa</a>
                        `;
                },
                width: "25%"
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
            deleteClient(url);
        }
    });
}

function deleteClient(url) {
    var currentPage = $('#clientTable').DataTable().page();

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
                var table = $('#clientTable').DataTable();
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
