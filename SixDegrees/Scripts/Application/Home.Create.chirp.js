﻿$(function () {
    $('body').on('click', '#filenamebutton', function () {
        $('input[id=file]').click();
    });
    $('input[id=file]').change(function () {
        var val = $(this).val();
        var file = val.split(/[\\/]/);
        $('#filenametext').val(file[file.length-1]);
    });
});