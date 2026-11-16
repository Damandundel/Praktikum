// Praktikum - клиентски скриптове (jQuery се зарежда в layout-а).
$(function () {
    // Отваряне и затваряне на менюто на малки екрани.
    $('.nav-toggle').on('click', function () {
        var isOpen = $('#site-nav').toggleClass('is-open').hasClass('is-open');
        $(this).attr('aria-expanded', isOpen);
    });

    // Падащи менюта: отварят се с клик, затварят се при клик навън или с Escape.
    function closeDropdowns(except) {
        $('.nav-dropdown.is-open').not(except).removeClass('is-open')
            .children('.nav-trigger').attr('aria-expanded', 'false');
    }

    $('.nav-trigger').on('click', function (event) {
        event.stopPropagation();
        var $dropdown = $(this).closest('.nav-dropdown');
        closeDropdowns($dropdown);
        var isOpen = $dropdown.toggleClass('is-open').hasClass('is-open');
        $(this).attr('aria-expanded', isOpen);
    });

    $(document).on('click', function (event) {
        if (!$(event.target).closest('.nav-dropdown').length) {
            closeDropdowns();
        }
    });

    $(document).on('keydown', function (event) {
        if (event.key === 'Escape') {
            closeDropdowns();
        }
    });

    // Бутоните за демо акаунти попълват формата за вход (парола и имейл са публични демо данни).
    $('.demo-fill').on('click', function () {
        $('#Email').val($(this).data('email'));
        $('#Password').val($(this).data('password'));
        $('#Password').trigger('focus');
    });

    // Плавно появяване на секции при превъртане.
    var revealItems = document.querySelectorAll('.reveal');
    if ('IntersectionObserver' in window && revealItems.length) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.12 });
        revealItems.forEach(function (item) { observer.observe(item); });
    } else {
        revealItems.forEach(function (item) { item.classList.add('is-visible'); });
    }

    initChat();
});

// Потвърждение преди необратими действия: <form data-confirm="Текст на въпроса">.
$(document).on('submit', 'form[data-confirm]', function (event) {
    if (!window.confirm($(this).data('confirm'))) {
        event.preventDefault();
    }
});

// ---------------------------------------------------------------- чат
// Страницата на чата периодично пита сървъра за нови съобщения (polling на 3 секунди).
function initChat() {
    var $chat = $('#chat');
    if (!$chat.length) {
        return;
    }

    var pollUrl = $chat.data('poll-url');
    var sendUrl = $chat.data('send-url');
    var lastId = parseInt($chat.data('last-id'), 10) || 0;
    var $messages = $('#chat-messages');
    var $text = $('#chat-text');
    var $form = $('#chat-form');
    var $error = $('#chat-error');

    function scrollDown() {
        $messages.scrollTop($messages[0].scrollHeight);
    }

    function addMessage(message) {
        if (message.id <= lastId) {
            return;
        }

        lastId = message.id;
        $('#chat-empty').remove();

        var $bubble = $('<div class="bubble"></div>').toggleClass('is-mine', message.isMine);
        $bubble.append(document.createTextNode(message.text));   // textContent: без HTML (защита от XSS)
        $bubble.append($('<time></time>').text(message.sentOn));
        $messages.append($bubble);
        scrollDown();
    }

    function poll() {
        if (document.hidden) {
            return;
        }

        $.getJSON(pollUrl, { after: lastId }).done(function (items) {
            items.forEach(addMessage);
        });
    }

    $form.on('submit', function (event) {
        event.preventDefault();
        var text = $.trim($text.val());
        if (!text) {
            return;
        }

        $error.text('');
        var token = $form.find('input[name="__RequestVerificationToken"]').val();

        $.ajax({
            url: sendUrl,
            method: 'POST',
            data: { text: text, __RequestVerificationToken: token },
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        }).done(function (message) {
            $text.val('');
            addMessage(message);
        }).fail(function (xhr) {
            $error.text((xhr.responseJSON && xhr.responseJSON.error) || 'Съобщението не можа да бъде изпратено.');
        });
    });

    $text.on('keydown', function (event) {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            $form.trigger('submit');
        }
    });

    scrollDown();
    setInterval(poll, 3000);
}
