$(document).ready(function () {
    $('#loaiHoaTable').DataTable({
        ajax: {
            url: '/Admin/LoaiHoa/List',
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
                width: "30%"
            },
            {
                data: 'description',
                width: "30%"
            },
            {
                data: 'id',
                render: (data, type, row) => {
                    return `
                            <a href="/Admin/LoaiHoa/Details/${data}" class="btn btn-dark btn-sm">Xem</a>
                            <a href="/Admin/LoaiHoa/Edit/${data}" class="btn btn-success btn-sm">Sửa</a>
                            <a onClick="confirmDelete('/Admin/LoaiHoa/Delete/${data}')" class="btn btn-danger btn-sm">Xóa</a>
                        `;
                },
                width: "30%"
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
            deleteLoaiHoa(url);
        }
    });
}

function deleteLoaiHoa(url) {
    var currentPage = $('#loaiHoaTable').DataTable().page();

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
                var table = $('#loaiHoaTable').DataTable();
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