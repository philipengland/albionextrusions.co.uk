/** Global Javascript Stylesheet **/

function onBodyLoad() {}

function externalLinks() {
    $('a[rel="external"]').each(function () {
        $(this).attr('target', '_blank');
    });
}

$(document).ready(function() {
	$('body').supersleight();
	externalLinks();
	
	onBodyLoad();
});