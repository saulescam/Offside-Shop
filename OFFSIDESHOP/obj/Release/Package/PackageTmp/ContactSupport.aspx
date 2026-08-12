<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ContactSupport.aspx.cs" Inherits="OFFSIDESHOP.ContactSupport" %>
<%@ Register Src="~/FooterControl.ascx" TagPrefix="uc" TagName="Footer" %>

<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_Title %>" /> | OffsideShop Support</title>

    <link rel="icon" type="image/x-icon" href="assets/favicon.ico" />
    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>
    <link href="https://fonts.googleapis.com/css?family=Montserrat:400,600,700" rel="stylesheet" type="text/css" />
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="SweetAlert/sweetalert2.all.min.js"></script>
    <link href="css/styles.css" rel="stylesheet" />
    
    <style>
        body { background-color: #f8f9fa; font-family: 'Montserrat', sans-serif; }
        
        .support-card {
            background-color: #ffffff;
            border: 1px solid #e0e0e0;
            border-radius: 12px;
            padding: 40px;
            box-shadow: 0px 4px 25px rgba(0,0,0,0.05);
            margin-bottom: 50px;
        }

        .form-label { font-weight: 600; color: #1a1a1a; margin-bottom: 8px; font-size: 0.95rem; }
        .form-control, .form-select { border: 1px solid #ced4da; border-radius: 6px; padding: 10px 15px; font-size: 0.95rem; transition: border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out; }
        .form-control:focus, .form-select:focus { border-color: #ffc800; box-shadow: 0 0 0 0.25rem rgba(255, 200, 0, 0.25); }
        .form-control[readonly] { background-color: #f1f3f5; cursor: not-allowed; }
        .form-text { color: #6c757d; font-size: 0.85rem; }

        .btn-submit {
            background-color: #1a1a1a;
            color: #ffffff;
            font-weight: 600;
            padding: 12px 30px;
            border-radius: 6px;
            border: 2px solid #1a1a1a;
            transition: all 0.3s ease;
            text-transform: uppercase;
            letter-spacing: 1px;
        }
        .btn-submit:hover { background-color: #ffc800; color: #1a1a1a; border-color: #ffc800; }

        .dynamic-panel {
            background-color: #fdfdfd;
            border-left: 4px solid #ffc800;
            padding: 20px;
            margin-top: 20px;
            margin-bottom: 20px;
            border-radius: 0 8px 8px 0;
            box-shadow: inset 0 0 10px rgba(0,0,0,0.02);
        }

        /* Ã¢â€â‚¬Ã¢â€â‚¬ COMPONENTE DRAG & DROP PREMIUM DESDE CERO Ã¢â€â‚¬Ã¢â€â‚¬ */
        .premium-drop-zone {
            display: block !important;
            border: 2px dashed #ced4da;
            border-radius: 12px;
            padding: 40px 20px;
            text-align: center;
            background-color: #fdfdfd;
            cursor: pointer;
            transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
            position: relative;
            box-shadow: 0 4px 6px rgba(0,0,0,0.01);
        }
        
        .premium-drop-zone:hover {
            border-color: #ffc800;
            background-color: #fffdf5;
            box-shadow: 0 6px 12px rgba(255, 200, 0, 0.05);
        }

        .premium-drop-zone.highlight {
            border-color: #ffc800;
            background-color: rgba(255, 200, 0, 0.08);
            transform: scale(1.01);
        }

        .premium-drop-zone i {
            color: #ced4da;
            transition: all 0.3s ease;
            transform: translateY(0);
        }

        .premium-drop-zone:hover i, .premium-drop-zone.highlight i {
            color: #ffc800;
            transform: translateY(-5px);
        }

        .premium-drop-zone h6 {
            color: #333333;
            font-weight: 700;
            margin-top: 15px;
            font-size: 1.05rem;
        }

        .premium-drop-zone p {
            font-size: 0.85rem;
            color: #6c757d;
            margin-bottom: 0;
        }

        .file-upload-input {
            display: none !important;
        }

        .thumbnail-container {
            transition: transform 0.2s ease, box-shadow 0.2s ease;
        }
        
        .thumbnail-container:hover {
            transform: scale(1.05);
            box-shadow: 0 4px 10px rgba(0,0,0,0.15);
        }
        
        .user-menu-container { position: relative; display: flex; align-items: center; margin-left: auto; }
        .user-icon-btn { background: none; border: none; cursor: pointer; padding: 8px; color: #ffffff; transition: all 0.3s ease; display: flex; align-items: center; justify-content: center; width: 40px; height: 40px; border-radius: 50%; }
        .user-icon-btn:hover { color: #FFC800; background-color: rgba(255, 200, 0, 0.1); }
        .user-dropdown-menu { position: absolute; top: 50px; right: 0; background: #1a1a1a; border: 1px solid #FFC800; border-radius: 8px; min-width: 260px; box-shadow: 0 4px 12px rgba(0, 0, 0, 0.5); z-index: 1000; padding: 0; }
        .user-info { padding: 12px 16px; border-bottom: 1px solid #333333; }
        .user-fullname { margin: 0; color: #FFC800; font-weight: bold; font-size: 0.95rem; }
        .user-email { margin: 4px 0 0 0; color: #888888; font-size: 0.8rem; }
        
        .dropdown-content { display: flex; flex-direction: column; padding: 8px 0; }
        .dropdown-item { display: flex; align-items: center; gap: 10px; padding: 10px 16px; color: #ffffff; text-decoration: none; cursor: pointer; border: none; background: transparent; width: 100%; text-align: left; transition: all 0.2s; font-size: 0.95rem; }
        .dropdown-item:hover { background-color: #FFC800; color: #000000; }
        .dropdown-item i { font-size: 1rem; width: 20px; }
        .dropdown-item.btn-logout { border-top: 1px solid #333333; margin-top: 4px; padding-top: 10px; }
        .dropdown-item.btn-logout:hover { background-color: #D47A00 !important; }
        .badge { margin-left: auto; background-color: #D47A00; color: white; padding: 2px 6px; border-radius: 10px; font-size: 0.75rem; min-width: 18px; text-align: center; }
    </style>

    <script type="text/javascript">
        function toggleUserMenu(button) {
            const container = button.closest('.user-menu-container');
            if (!container) return;
            const menu = container.querySelector('.dynamic-dropdown');
            if (!menu) return;
            if (menu.style.display === 'block') { menu.style.display = 'none'; }
            else { cerrarTodosLosMenus(); menu.style.display = 'block'; }
        }
        function cerrarTodosLosMenus() {
            const menus = document.querySelectorAll('.dynamic-dropdown');
            menus.forEach(m => m.style.display = 'none');
        }
        document.onclick = function (event) {
            const container = event.target.closest('.user-menu-container');
            if (!container) { cerrarTodosLosMenus(); }
        };

        // DYNAMIC PREMIUM FILE UPLOADER SYSTEM
        let uploadedFilesArray = [];
        let isUpdatingFiles = false;

        function initDropArea() {
            let dropArea = document.getElementById('drop-area');
            let fileInput = document.getElementById('fileImages') || document.querySelector('.file-upload-input');

            if (!dropArea || !fileInput) return;
            if (dropArea.dataset.initialized === "true") return;
            dropArea.dataset.initialized = "true";

            // Visual highlight on drag events
            ['dragenter', 'dragover'].forEach(eventName => {
                dropArea.addEventListener(eventName, e => {
                    e.preventDefault();
                    e.stopPropagation();
                    dropArea.classList.add('highlight');
                }, false);
            });

            ['dragleave', 'drop'].forEach(eventName => {
                dropArea.addEventListener(eventName, e => {
                    e.preventDefault();
                    e.stopPropagation();
                    dropArea.classList.remove('highlight');
                }, false);
            });

            // Prevent default drag and drop behavior on window to avoid opening files in browser
            window.addEventListener('dragover', e => e.preventDefault(), false);
            window.addEventListener('drop', e => e.preventDefault(), false);

            // Handle file drops
            dropArea.addEventListener('drop', function (e) {
                if (e.dataTransfer && e.dataTransfer.files && e.dataTransfer.files.length > 0) {
                    addFiles(e.dataTransfer.files, true);
                }
            }, false);

            // Handle file inputs via explorer selection
            fileInput.addEventListener('change', function () {
                if (isUpdatingFiles) return;
                if (this.files && this.files.length > 0) {
                    addFiles(this.files, false);
                }
            });
        }

        function addFiles(files, updateFileInput) {
            let fileInput = document.getElementById('fileImages') || document.querySelector('.file-upload-input');
            if (!fileInput) return;

            let fileList = Array.from(files);
            if (fileList.length === 0) return;

            // Validate file limit count
            if (fileList.length > 3) {
                Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_Contact_LimitReachedTitle") %>', '<%= GetGlobalResourceObject("Strings", "Alert_Contact_LimitReachedText") %>', 'warning');
                return;
            }

            let allowedExtensions = ['.jpg', '.jpeg', '.png', '.webp'];
            let maxSizeBytes = 2 * 1024 * 1024; // 2MB

            for (let file of fileList) {
                let ext = '.' + file.name.split('.').pop().toLowerCase();
                if (!allowedExtensions.includes(ext)) {
                    let msg = '<%= GetGlobalResourceObject("Strings", "Alert_Contact_FormatErrorText") %>'.replace('{0}', file.name);
                    Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_Contact_FormatErrorTitle") %>', msg, 'error');
                    return;
                }
                if (file.size > maxSizeBytes) {
                    let msg = '<%= GetGlobalResourceObject("Strings", "Alert_Contact_TooLargeText") %>'.replace('{0}', file.name);
                    Swal.fire('<%= GetGlobalResourceObject("Strings", "Alert_Contact_TooLargeTitle") %>', msg, 'error');
                    return;
                }
            }

            uploadedFilesArray = fileList;
            renderPreviewAndSync(updateFileInput);
        }

        function removeFile(index) {
            uploadedFilesArray.splice(index, 1);
            renderPreviewAndSync(true);
        }

        function renderPreviewAndSync(updateFileInput) {
            let fileInput = document.getElementById('fileImages') || document.querySelector('.file-upload-input');
            let gallery = document.getElementById('gallery');
            if (!fileInput || !gallery) return;

            if (updateFileInput) {
                try {
                    isUpdatingFiles = true;
                    let dt = new DataTransfer();
                    uploadedFilesArray.forEach(file => dt.items.add(file));
                    fileInput.files = dt.files;
                } catch (e) {
                    console.error("DataTransfer sync failed: ", e);
                } finally {
                    isUpdatingFiles = false;
                }
            }

            // Render thumbnails with dynamic removal trigger
            gallery.innerHTML = '';
            uploadedFilesArray.forEach((file, index) => {
                let reader = new FileReader();
                reader.readAsDataURL(file);
                reader.onload = function (e) {
                    let container = document.createElement('div');
                    container.className = 'position-relative m-2 thumbnail-container';
                    container.style.width = '100px';
                    container.style.height = '100px';
                    container.style.display = 'inline-block';
                    
                    let img = document.createElement('img');
                    img.src = e.target.result;
                    img.className = 'img-thumbnail w-100 h-100 shadow-sm';
                    img.style.objectFit = 'cover';
                    img.style.borderRadius = '8px';
                    img.style.border = '2px solid #ffc800';
                    
                    let removeBtn = document.createElement('button');
                    removeBtn.type = 'button';
                    removeBtn.className = 'btn btn-danger btn-sm rounded-circle d-flex align-items-center justify-content-center';
                    removeBtn.style.position = 'absolute';
                    removeBtn.style.top = '-8px';
                    removeBtn.style.right = '-8px';
                    removeBtn.style.width = '22px';
                    removeBtn.style.height = '22px';
                    removeBtn.style.padding = '0';
                    removeBtn.style.border = '1px solid white';
                    removeBtn.style.fontSize = '12px';
                    removeBtn.style.fontWeight = 'bold';
                    removeBtn.style.zIndex = '10';
                    removeBtn.innerHTML = '&times;';
                    
                    removeBtn.addEventListener('click', function (event) {
                        event.preventDefault();
                        event.stopPropagation();
                        removeFile(index);
                    });

                    container.appendChild(img);
                    container.appendChild(removeBtn);
                    gallery.appendChild(container);
                };
            });
        }

        document.addEventListener('DOMContentLoaded', initDropArea);
        if (typeof Sys !== 'undefined') {
            Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(initDropArea);
        }
    </script>
</head>
<body id="page-top" style="display: flex; flex-direction: column; min-height: 100vh; margin: 0;"> 
    <form runat="server" style="flex: 1 0 auto; display: flex; flex-direction: column;">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <script type="text/javascript">
            Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function (sender, args) {
                if (typeof initDropArea === 'function') {
                    initDropArea();
                }
            });
        </script>
        
        <!-- Navigation Bar -->
        <nav class="navbar navbar-expand-lg navbar-dark fixed-top" id="mainNav" style="background-color: #1a1a1a !important; box-shadow: 0 2px 10px rgba(0,0,0,0.3); padding: 12px 0;">
            <div class="container">
                <a class="navbar-brand" href="Homepage.aspx">
                    <img src="assets/img/offsideshop_logo_white_letras.png" alt="OffsideShop Logo" style="max-height: 45px; width: auto;" />
                </a>
                <asp:LinkButton ID="btnLanguageToggle" runat="server" OnClick="btnLanguageToggle_Click" CssClass="lang-switcher" style="color: #fff; text-decoration: none; font-weight: bold; margin-left: 10px; margin-right: auto;">EN / ES</asp:LinkButton>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarResponsive">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarResponsive">
                    
                    <asp:PlaceHolder ID="phNavbarGuest" runat="server">
                        <div class="user-menu-container">
                            <button type="button" class="user-icon-btn" onclick="toggleUserMenu(this)">
                                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="8" r="4"></circle><path d="M 6 20c0-4 2.5-6 6-6s6 2 6 6"></path></svg>
                            </button>
                            <div class="user-dropdown-menu dynamic-dropdown" style="display: none;">
                                <div class="dropdown-content">
                                    <a href="Login.aspx" class="dropdown-item"><i class="fas fa-sign-in-alt"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_Login %>" /></a>
                                    <a href="SignUp.aspx" class="dropdown-item"><i class="fas fa-user-plus"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_SignUp %>" /></a>
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>
                    <asp:PlaceHolder ID="phNavbarUser" runat="server">
                        <div class="user-menu-container">
                            <asp:UpdatePanel ID="upPerfil" runat="server" UpdateMode="Conditional">
                                <ContentTemplate>
                                    <button type="button" class="user-icon-btn" onclick="toggleUserMenu(this)">
                                        <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="8" r="4"></circle><path d="M 6 20c0-4 2.5-6 6-6s6 2 6 6"></path></svg>
                                    </button>
                                    <div id="userDropdownMenuUser" class="user-dropdown-menu dynamic-dropdown" style="display: none;">
                                        <div class="user-info">
                                            <p class="user-fullname"><asp:Label ID="lblFullName" runat="server" Text="<%$ Resources:Strings, Account_Loading %>"></asp:Label></p>
                                            <p class="user-email"><asp:Label ID="lblUserEmail" runat="server" Text=""></asp:Label></p>
                                        </div>
                                        <div class="dropdown-content">
                                            <asp:LinkButton ID="btnGoToAccount" runat="server" CssClass="dropdown-item" OnClick="btnGoToAccount_Click"><i class="fas fa-user-cog"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_MyAccount %>" /></asp:LinkButton>
                                            <asp:LinkButton ID="btnMyOrders" runat="server" CssClass="dropdown-item" OnClick="btnMyOrders_Click"><i class="fas fa-clipboard-list"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_MyOrders %>" /></asp:LinkButton>
                                            <asp:LinkButton ID="btnNavCart" runat="server" CssClass="dropdown-item" OnClick="btnNavCart_Click"><i class="fas fa-shopping-cart"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_Cart %>" /> <span class="badge"><asp:Label ID="lblCartCount" runat="server" Text="0"></asp:Label></span></asp:LinkButton>
                                            <asp:Button ID="btncerrar" runat="server" CssClass="dropdown-item btn-logout" Text="<%$ Resources:Strings, Nav_LogOut %>" OnClick="btncerrar_Click" />
                                        </div>
                                    </div>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </div>
                    </asp:PlaceHolder>
                    <asp:PlaceHolder ID="phNavbarAdmin" runat="server">
                        <div class="user-menu-container">
                            <button type="button" class="user-icon-btn" onclick="toggleUserMenu(this)">
                                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="8" r="4"></circle><path d="M 6 20c0-4 2.5-6 6-6s6 2 6 6"></path></svg>
                            </button>
                            <div class="user-dropdown-menu dynamic-dropdown" style="display: none;">
                                <div class="user-info">
                                    <p class="user-fullname"><asp:Label ID="lblAdminName" runat="server" Text="Admin"></asp:Label></p>
                                    <p class="user-role"><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_AdminRole %>" /></p>
                                </div>
                                <div class="dropdown-content">
                                    <a href="MyAccount.aspx" class="dropdown-item"><i class="fas fa-user-cog"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_MyAccount %>" /></a>
                                    <a href="Dashboard.aspx" class="dropdown-item"><i class="fas fa-chart-line"></i><asp:Literal runat="server" Text="<%$ Resources:Strings, Nav_Dashboard %>" /></a>
                                    <asp:Button ID="btnlogout" runat="server" CssClass="dropdown-item btn-logout" Text="<%$ Resources:Strings, Nav_LogOut %>" OnClick="btncerrar_Click" />
                                </div>
                            </div>
                        </div>
                    </asp:PlaceHolder>
                </div>
            </div>
        </nav>
        
        <div style="margin-top: 120px;"></div>

        <!-- Main Form Content -->
        <div class="container pb-5">
            <div class="row justify-content-center">
                <div class="col-lg-8">
                    
                    <h2 class="mb-4" style="font-weight: 700; color: #1a1a1a;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_Title %>" /></h2>
                    
                    <div class="support-card">
                        <asp:UpdatePanel ID="upForm" runat="server">
                            <ContentTemplate>
                                
                                <!-- Reason Dropdown -->
                                <div class="mb-4">
                                    <label class="form-label"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_HelpWith %>" /> <span class="text-danger">*</span></label>
                                    <asp:DropDownList ID="ddlReason" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlReason_SelectedIndexChanged">
                                        <asp:ListItem Text="-" Value=""></asp:ListItem>
                                    </asp:DropDownList>
                                    <div class="form-text"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_HelpDesc %>" /></div>
                                </div>

                                <div class="mb-4">
                                    <label class="form-label"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_EmailLabel %>" /> <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtEmail" runat="server" CssClass="form-control" ReadOnly="true" placeholder="<%$ Resources:Strings, Contact_EmailPlaceholder %>"></asp:TextBox>
                                </div>

                                <div class="mb-4">
                                    <label class="form-label"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_Subject %>" /> <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtSubject" runat="server" CssClass="form-control" required="required"></asp:TextBox>
                                </div>

                                <!-- Dynamic Panel: Order Issue -->
                                <asp:Panel ID="pnlOrderIssue" runat="server" Visible="false" CssClass="dynamic-panel">
                                    <div class="mb-3">
                                        <label class="form-label"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_OrderId %>" /> <span class="text-danger">*</span></label>
                                        <asp:TextBox ID="txtOrderId" runat="server" CssClass="form-control" placeholder="<%$ Resources:Strings, Contact_OrderIdPlaceholder %>" TextMode="Number" min="1"></asp:TextBox>
                                        <div class="form-text"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_OrderIdDesc %>" /></div>
                                    </div>
                                </asp:Panel>

                                <!-- Dynamic Panel: Sell Collector's Jersey -->
                                <asp:Panel ID="pnlSellJersey" runat="server" Visible="false" CssClass="dynamic-panel">
                                    <h5 class="mb-3" style="font-weight: 700;"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_ConsignTitle %>" /></h5>
                                    <div class="row">
                                        <div class="col-md-4 mb-3">
                                            <label class="form-label"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_Condition %>" /> <span class="text-danger">*</span></label>
                                            <asp:TextBox ID="txtCondition" runat="server" CssClass="form-control" placeholder="1 to 10" TextMode="Number" min="1" max="10"></asp:TextBox>
                                        </div>
                                        <div class="col-md-4 mb-3">
                                            <label class="form-label"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_Size %>" /> <span class="text-danger">*</span></label>
                                            <asp:DropDownList ID="ddlSize" runat="server" CssClass="form-select">
                                                <asp:ListItem Text="-" Value=""></asp:ListItem>
                                                <asp:ListItem Text="S" Value="S"></asp:ListItem>
                                                <asp:ListItem Text="M" Value="M"></asp:ListItem>
                                                <asp:ListItem Text="L" Value="L"></asp:ListItem>
                                                <asp:ListItem Text="XL" Value="XL"></asp:ListItem>
                                                <asp:ListItem Text="XXL" Value="XXL"></asp:ListItem>
                                            </asp:DropDownList>
                                        </div>
                                        <div class="col-md-4 mb-3">
                                            <label class="form-label"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_Price %>" /> <span class="text-danger">*</span></label>
                                            <asp:TextBox ID="txtPrice" runat="server" CssClass="form-control" TextMode="Number" step="0.01" min="0.01" placeholder="0.00"></asp:TextBox>
                                        </div>
                                    </div>

                                    <div class="mb-3">
                                        <label class="form-label"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_UploadTitle %>" /> <span class="text-danger">*</span></label>
                                        <label for="fileImages" id="drop-area" class="premium-drop-zone">
                                            <i class="fas fa-cloud-upload-alt fa-3x"></i>
                                            <h6><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_UploadDrag %>" /></h6>
                                            <p><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_UploadClick %>" /></p>
                                        </label>
                                        <asp:FileUpload ID="fileImages" runat="server" ClientIDMode="Static" AllowMultiple="true" CssClass="file-upload-input" accept=".jpg,.jpeg,.png,.webp" />
                                        <div id="gallery" class="mt-3 d-flex justify-content-center gap-3 flex-wrap"></div>
                                        <div class="form-text text-center mt-2"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_UploadDesc %>" /></div>
                                    </div>
                                </asp:Panel>

                                <div class="mb-4">
                                    <label class="form-label"><asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_DetailsTitle %>" /> <span class="text-danger">*</span></label>
                                    <asp:TextBox ID="txtMessage" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="5" required="required" placeholder="<%$ Resources:Strings, Contact_DetailsPlaceholder %>"></asp:TextBox>
                                </div>

                            </ContentTemplate>
                            <Triggers>
                                <asp:PostBackTrigger ControlID="btnSubmit" />
                            </Triggers>
                        </asp:UpdatePanel>

                        <div class="d-flex justify-content-between align-items-center mt-4 pt-3" style="border-top: 1px solid #e0e0e0;">
                            <a href="Homepage.aspx" class="btn btn-outline-secondary px-4 py-2 font-weight-bold" style="border-radius: 6px; font-size: 0.95rem; text-transform: uppercase; letter-spacing: 0.5px; border-width: 2px;">
                                <i class="fas fa-arrow-left mr-2"></i> <asp:Literal runat="server" Text="<%$ Resources:Strings, Contact_BackHome %>" />
                            </a>
                            <asp:Button ID="btnSubmit" runat="server" CssClass="btn-submit" Text="<%$ Resources:Strings, Contact_SubmitBtn %>" OnClick="btnSubmit_Click" />
                        </div>
                    </div>
                    
                </div>
            </div>
        </div>

    </form>
    
    <uc:Footer ID="ControlFooter" runat="server" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>

