<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ChatbotControl.ascx.cs" Inherits="OFFSIDESHOP.ChatbotControl" %>

<!-- BOTÓN FLOTANTE -->
<button type="button" id="chat-widget-button" onclick="toggleChatbot()" title="Chat with Offside Assistant">
    <img src="assets/img/offside-bot.png" alt="Bot" style="width: 40px; height: 40px; border-radius: 50%; object-fit: cover;" />
</button>

<!-- VENTANA DEL CHAT -->
<div id="chat-widget-container" class="shadow-lg">
    <div class="chat-header d-flex justify-content-between align-items-center">
        <div class="d-flex align-items-center gap-2">
            <img src="assets/img/offside-bot.png" alt="Bot Avatar" style="width: 30px; height: 30px; border-radius: 50%; object-fit: cover;" />
            <div>
                <h6 class="mb-0 text-white fw-bold" style="font-family: 'Raleway', sans-serif;">OFFSIDE Assistant</h6>
                <small class="text-success" style="font-size: 0.7rem;">● Online</small>
            </div>
        </div>
        <button type="button" class="btn-close-chat" onclick="toggleChatbot()">&times;</button>
    </div>

    <div class="chat-body" id="chat-body-messages">
        <div class="chat-message bot-message">
            <asp:Literal runat="server" Text="<%$ Resources:Strings, ChatBot_HI %>" />
        </div>
    </div>
<div class="chat-footer">
        <div class="input-group">
            <input type="text" id="chat-user-input" class="form-control" placeholder='<%= GetGlobalResourceObject("Strings", "ChatBot_Ask") %>' onkeydown="return handleChatKeyPress(event);" />
            <div class="input-group-append">
                <button class="btn btn-warning" type="button" id="btn-send-chat" onclick="sendChatMessage()" style="font-weight: bold; background: #FFC800; border: none; color: #000;">
                    <i class="fas fa-paper-plane"></i>
                </button>
            </div>
        </div>
    </div>
</div>

<style>
    #chat-widget-button {
        position: fixed;
        bottom: 25px;
        right: 25px;
        width: 65px;
        height: 65px;
        border-radius: 50%;
        background: #1a1a1a;
        border: 2px solid #FFC800;
        box-shadow: 0 5px 20px rgba(0,0,0,0.5);
        cursor: pointer;
        z-index: 9999;
        transition: transform 0.3s ease, box-shadow 0.3s ease;
        display: flex;
        align-items: center;
        justify-content: center;
        padding: 0;
    }

    #chat-widget-button:hover {
        transform: scale(1.08);
        box-shadow: 0 8px 25px rgba(255, 200, 0, 0.4);
    }

    #chat-widget-container {
        position: fixed;
        bottom: 100px;
        right: 25px;
        width: 340px;
        height: 480px;
        background-color: #121212;
        border: 1px solid #333;
        border-radius: 12px;
        display: none;
        flex-direction: column;
        z-index: 9999;
        overflow: hidden;
        box-shadow: 0 10px 30px rgba(0,0,0,0.7);
    }

    .chat-header {
        background-color: #0a0a0a;
        padding: 12px 16px;
        border-bottom: 1px solid #222;
    }

    .btn-close-chat {
        background: none;
        border: none;
        color: #aaa;
        font-size: 1.5rem;
        cursor: pointer;
    }

    .btn-close-chat:hover {
        color: #FFC800;
    }

    .chat-body {
        flex: 1;
        padding: 15px;
        overflow-y: auto;
        display: flex;
        flex-direction: column;
        gap: 12px;
    }

    .chat-message {
        max-width: 85%;
        padding: 10px 14px;
        border-radius: 10px;
        font-size: 0.85rem;
        line-height: 1.4;
        word-wrap: break-word;
        font-family: 'Raleway', sans-serif;
    }

    .bot-message {
        background-color: #2a2a2a;
        color: #fff;
        align-self: flex-start;
        border-bottom-left-radius: 2px;
    }

    .user-message {
        background-color: #FFC800;
        color: #000;
        font-weight: 600;
        align-self: flex-end;
        border-bottom-right-radius: 2px;
    }

    .chat-footer {
        padding: 12px;
        background-color: #0a0a0a;
        border-top: 1px solid #222;
    }

    #chat-user-input {
        background-color: #222;
        border: 1px solid #333;
        color: #fff;
    }

    #chat-user-input:focus {
        border-color: #FFC800;
        box-shadow: none;
    }
</style>

<script type="text/javascript">
    function toggleChatbot() {
        var container = document.getElementById('chat-widget-container');
        if (container.style.display === 'none' || container.style.display === '') {
            container.style.display = 'flex';
            document.getElementById('chat-user-input').focus();
        } else {
            container.style.display = 'none';
        }
    }

    function handleChatKeyPress(e) {
        // Detecta la tecla Enter (código 13)
        if (e.keyCode === 13 || e.which === 13) {
            e.preventDefault(); // Detiene el comportamiento por defecto
            sendChatMessage();  // Envía el mensaje al bot
            return false;       // BLOQUEA el PostBack de ASP.NET
        }
        return true;
    }

    function sendChatMessage() {
        var input = document.getElementById('chat-user-input');
        var message = input.value.trim();
        if (!message) return;

        var chatBody = document.getElementById('chat-body-messages');

        // Render User Message
        var userDiv = document.createElement('div');
        userDiv.className = 'chat-message user-message';
        userDiv.innerText = message;
        chatBody.appendChild(userDiv);

        input.value = '';
        chatBody.scrollTop = chatBody.scrollHeight;

        // Render Typing Indicator
        var loadingDiv = document.createElement('div');
        loadingDiv.className = 'chat-message bot-message';
        loadingDiv.id = 'chat-loading-indicator';
        loadingDiv.innerHTML = '<i class="fas fa-ellipsis-h fa-fade" style="color: #FFC800;"></i>';
        chatBody.appendChild(loadingDiv);
        chatBody.scrollTop = chatBody.scrollHeight;

        // Capturar la URL actual donde está el usuario
        var currentUrl = window.location.href;

        // AJAX Call to Handler
        fetch('ChatHandler.ashx', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ message: message, url: currentUrl })
        })
            .then(response => response.json())
            .then(data => {
                document.getElementById('chat-loading-indicator').remove();
                var botDiv = document.createElement('div');
                botDiv.className = 'chat-message bot-message';
                botDiv.innerText = data.reply;
                chatBody.appendChild(botDiv);
                chatBody.scrollTop = chatBody.scrollHeight;
            })
            .catch(error => {
                document.getElementById('chat-loading-indicator').remove();
                var errDiv = document.createElement('div');
                errDiv.className = 'chat-message bot-message text-danger';
                errDiv.innerText = "Connection lost. Please try again.";
                chatBody.appendChild(errDiv);
                chatBody.scrollTop = chatBody.scrollHeight;
            });
    }
</script>