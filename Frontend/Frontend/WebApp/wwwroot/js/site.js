document.addEventListener("DOMContentLoaded", () => {
  initQuills();
  initModals();
  initDropdowns();
  initFileUploads();
  initCustomSelects();
  initDarkMode();
});

function initQuills() {
  document.querySelectorAll("[data-quill-editor]").forEach((editor) => {
    const editorId = editor.id;
    const textarea = document.querySelector(
      `[data-quill-textarea="#${editorId}"]`
    );
    const toolbarId = editor.getAttribute("data-quill-toolbar");

    const quill = new Quill(`#${editorId}`, {
      modules: {
        syntax: true,
        toolbar: toolbarId,
      },
      theme: "snow",
      placeholder: "Skriv något...",
    });

    if (textarea) {
      quill.on("text-change", () => {
        textarea.value = quill.root.innerHTML;
      });
    }
  });
}

function initModals() {
  document.querySelectorAll('[data-type="modal"]').forEach((trigger) => {
    const target = document.querySelector(trigger.getAttribute("data-target"));

    trigger.addEventListener("click", () => {
      target?.classList.add("modal-show");
    });
  });

  document.querySelectorAll('[data-type="close"]').forEach((btn) => {
    const target = document.querySelector(btn.getAttribute("data-target"));

    btn.addEventListener("click", () => {
      target?.classList.remove("modal-show");
    });
  });
}

function initDropdowns() {
  document.addEventListener("click", (e) => {
    let clickedInsideDropdown = false;

    document
      .querySelectorAll('[data-type="dropdown"]')
      .forEach((dropdownTrigger) => {
        const targetId = dropdownTrigger.getAttribute("data-target");
        const dropdown = document.querySelector(targetId);

        if (dropdownTrigger.contains(e.target)) {
          clickedInsideDropdown = true;

          document
            .querySelectorAll(".dropdown.dropdown-show")
            .forEach((open) => {
              if (open !== dropdown) open.classList.remove("dropdown-show");
            });

          dropdown?.classList.toggle("dropdown-show");
        }
      });

    if (!clickedInsideDropdown && !e.target.closest(".dropdown")) {
      document.querySelectorAll(".dropdown.dropdown-show").forEach((open) => {
        open.classList.remove("dropdown-show");
      });
    }
  });
}

function initFileUploads() {
  document.querySelectorAll("[data-file-upload]").forEach((container) => {
    const input = container.querySelector('input[type="file"]');
    const preview = container.querySelector("img");
    const iconContainer = container.querySelector("circle");
    const icon = iconContainer?.querySelector("i");

    container.addEventListener("click", () => {
      input?.click();
    });

    input?.addEventListener("change", (e) => {
      const file = e.target.files[0];

      if (file && file.type.startsWith("image/")) {
        const reader = new FileReader();

        reader.onload = () => {
          preview.src = reader.result;
          preview.classList.remove("hide");
          iconContainer?.classList.add("selected");
          icon?.classList.replace("fa-camera", "fa-pen-to-square");
        };

        reader.readAsDataURL(file);
      }
    });
  });
}

function initCustomSelects() {
  document.querySelectorAll(".form-select").forEach((select) => {
    const trigger = select.querySelector(".form-select-trigger");
    const triggerText = trigger.querySelector(".form-select-text");
    const options = select.querySelectorAll(".form-select-option");
    const hiddenInput = select.querySelector('input[type="hidden"]');
    const placeholder = select.dataset.placeholder || "Välj";

    const setValue = (value = "", text = placeholder) => {
      triggerText.textContent = text;
      hiddenInput.value = value;
      select.classList.toggle("has-placeholder", !value);
    };

    setValue();

    trigger.addEventListener("click", (e) => {
      e.stopPropagation();

      document.querySelectorAll(".form-select.open").forEach((el) => {
        if (el !== select) el.classList.remove("open");
      });

      select.classList.toggle("open");
    });

    options.forEach((option) => {
      option.addEventListener("click", () => {
        setValue(option.dataset.value, option.textContent);
        select.classList.remove("open");
      });
    });

    document.addEventListener("click", (e) => {
      if (!select.contains(e.target)) {
        select.classList.remove("open");
      }
    });
  });
}

function initDarkMode() {
  const darkModeToggle = document.getElementById('darkModeToggle');
  const isDarkMode = localStorage.getItem('darkMode') === 'true';
  
  // Set initial state
  if (isDarkMode) {
    document.body.classList.add('dark-mode');
    darkModeToggle.checked = true;
  }

  // Handle toggle
  darkModeToggle.addEventListener('change', (e) => {
    const isDark = e.target.checked;
    document.body.classList.toggle('dark-mode', isDark);
    localStorage.setItem('darkMode', isDark);
  });
}

// Add project deletion handler
$(document).on('click', '.project .remove', function (e) {
    e.preventDefault();
    var projectId = $(this).closest('.project').data('project-id');
    if (confirm('Are you sure you want to delete this project?')) {
        $.ajax({
            url: '/admin/projects/delete/' + projectId,
            type: 'POST',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (result) {
                location.reload();
            },
            error: function (xhr, status, error) {
                alert('Error deleting project.');
            }
        });
    }
});

// Add project form submission handler
$(document).on('submit', '#add-project-form', function(e) {
    e.preventDefault();
    
    // Clear any existing error messages
    $('.text-danger').text('');
    
    // Get the form data
    var formData = new FormData(this);
    
    // Add the Quill editor content to the form data
    var description = $('#add-project-description-wysiwyg-editor').find('.ql-editor').html();
    formData.set('Description', description);
    
    // Log the form data for debugging
    console.log('Form data being sent:');
    for (var pair of formData.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
    }
    
    $.ajax({
        url: $(this).attr('action'),
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(result) {
            if (result.success) {
                // Close the modal
                $('#add-project-modal').removeClass('modal-show');
                // Reload the page to show the new project
                location.reload();
            } else {
                // Display validation errors
                if (result.errors && result.errors.length > 0) {
                    result.errors.forEach(function(error) {
                        // Find the closest form group and append the error
                        var errorSpan = $('<span class="text-danger"></span>').text(error);
                        $('.form-group').first().append(errorSpan);
                    });
                }
            }
        },
        error: function(xhr, status, error) {
            console.error('Error submitting form:', error);
            console.error('Status:', status);
            console.error('Response:', xhr.responseText);
            alert('Error creating project. Please try again.');
        }
    });
});

// Handle edit project button click
$(document).on('click', '[data-type="modal"][data-target="#edit-project-modal"]', function() {
    var projectId = $(this).closest('.project').data('project-id');
    
    // Show the modal first
    $('#edit-project-modal').addClass('modal-show');
    
    // Initialize Quill if not already initialized
    const quillContainer = $('#edit-project-description-wysiwyg-editor');
    let quillEditor = quillContainer[0].__quill;
    
    console.log('Edit Project Modal:', {
        'Modal Shown': true,
        'Quill Container Found': quillContainer.length > 0,
        'Quill Already Initialized': !!quillEditor,
        'Modal Element': $('#edit-project-modal').length > 0
    });
    
    if (!quillEditor) {
        // Wait for modal to be shown and DOM to be ready
        setTimeout(() => {
            const toolbarElement = $('#edit-project-description-toolbar');
            const editorElement = $('#edit-project-description-wysiwyg-editor');
            
            console.log('Initializing Quill Editor:', {
                'Toolbar Element Found': toolbarElement.length > 0,
                'Toolbar Element HTML': toolbarElement.length ? toolbarElement.html() : 'Not found',
                'Editor Element Found': editorElement.length > 0,
                'Modal Visible': $('#edit-project-modal').is(':visible'),
                'Modal Classes': $('#edit-project-modal').attr('class')
            });
            
            quillEditor = new Quill('#edit-project-description-wysiwyg-editor', {
                modules: {
                    syntax: true,
                    toolbar: '#edit-project-description-toolbar'
                },
                theme: 'snow',
                placeholder: 'Skriv något...'
            });
            quillContainer[0].__quill = quillEditor;
            
            console.log('Quill Editor Initialized:', {
                'Editor Instance Created': !!quillEditor,
                'Toolbar Module Loaded': !!quillEditor.getModule('toolbar'),
                'Toolbar Element Now Found': $('#edit-project-description-toolbar').length > 0
            });
            
            // Get project data after Quill is initialized
            loadProjectData(projectId, quillEditor);
        }, 200); // Increased delay to ensure DOM is ready
    } else {
        console.log('Using Existing Quill Editor:', {
            'Editor Instance': !!quillEditor,
            'Toolbar Module Loaded': !!quillEditor.getModule('toolbar'),
            'Toolbar Element Found': $('#edit-project-description-toolbar').length > 0
        });
        
        // If Quill is already initialized, load project data immediately
        loadProjectData(projectId, quillEditor);
    }
});

// Function to load project data
function loadProjectData(projectId, quillEditor) {
    $.ajax({
        url: '/admin/projects/get/' + projectId,
        type: 'GET',
        success: function(result) {
            console.log('Received project data:', result);
            const form = $('#edit-project-form');
            
            // Set basic form values
            form.find('[name="Id"]').val(result.id);
            form.find('[name="ProjectName"]').val(result.projectName);
            form.find('[name="StartDate"]').val(result.startDate);
            form.find('[name="EndDate"]').val(result.endDate);
            form.find('[name="Budget"]').val(result.budget);
            
            // Set client value
            const clientSelect = form.find('.form-select').first();
            const clientOption = clientSelect.find('.form-select-option').filter(function() {
                return $(this).data('value').toLowerCase() === result.clientId.toLowerCase();
            });
            
            if (clientOption.length) {
                clientSelect.find('.form-select-text').text(clientOption.text());
                clientSelect.find('input[type="hidden"]').val(result.clientId);
                clientSelect.removeClass('has-placeholder');
            }
            
            // Set member value
            const memberSelect = form.find('.form-select').eq(1);
            const memberOption = memberSelect.find('.form-select-option').filter(function() {
                return $(this).data('value').toLowerCase() === result.memberId.toLowerCase();
            });
            
            if (memberOption.length) {
                memberSelect.find('.form-select-text').text(memberOption.text());
                memberSelect.find('input[type="hidden"]').val(result.memberId);
                memberSelect.removeClass('has-placeholder');
            }
            
            // Set status value
            const statusSelect = form.find('.form-select').last();
            const statusOption = statusSelect.find('.form-select-option').filter(function() {
                return $(this).data('value').toLowerCase() === result.statusId.toLowerCase();
            });
            
            if (statusOption.length) {
                statusSelect.find('.form-select-text').text(statusOption.text());
                statusSelect.find('input[type="hidden"]').val(result.statusId);
                statusSelect.removeClass('has-placeholder');
            }
            
            // Set image preview if exists
            if (result.projectImage) {
                const imagePreview = form.find('.image-preview-container img');
                imagePreview.attr('src', result.projectImage).removeClass('hide');
                form.find('.image-preview-container .circle').addClass('hide');
            }
            
            // Set description in Quill editor
            const descriptionTextarea = form.find('#edit-project-description');
            
            console.log('Description handling:');
            console.log('1. Raw description from server:', result.description);
            console.log('2. Description textarea found:', descriptionTextarea.length > 0);
            console.log('3. Quill editor found:', !!quillEditor);
            
            if (quillEditor) {
                // Set the textarea value
                descriptionTextarea.val(result.description);
                console.log('4. Textarea value after setting:', descriptionTextarea.val());
                
                // Set the Quill editor content
                quillEditor.root.innerHTML = result.description || '';
                console.log('5. Quill editor content after setting:', quillEditor.root.innerHTML);
                
                // Update the textarea with the formatted content
                descriptionTextarea.val(quillEditor.root.innerHTML);
                console.log('6. Final textarea value:', descriptionTextarea.val());
                
                // Force Quill to update its display
                quillEditor.update();
                console.log('7. Quill editor content after update:', quillEditor.root.innerHTML);
            } else {
                console.error('Quill editor not found!');
            }
        },
        error: function(xhr, status, error) {
            console.error('Error loading project data:', error);
            alert('Error loading project data. Please try again.');
        }
    });
}

// Edit project form submission handler
$(document).on('submit', '#edit-project-form', function(e) {
    e.preventDefault();
    
    // Clear any existing error messages
    $('.text-danger').text('');
    
    // Get the form data
    var formData = new FormData(this);
    
    // Remove invariant fields
    formData.delete('__Invariant');
    
    // Add the Quill editor content to the form data
    var description = $('#edit-project-description-wysiwyg-editor').find('.ql-editor').html();
    formData.set('Description', description);
    
    // Log the form data for debugging
    console.log('Form data being sent:');
    for (var pair of formData.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
    }
    
    $.ajax({
        url: '/admin/projects/edit',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(result) {
            if (result.success) {
                // Close the modal
                $('#edit-project-modal').removeClass('modal-show');
                // Reload the page to show the updated project
                location.reload();
            } else {
                // Display validation errors
                if (result.errors && result.errors.length > 0) {
                    result.errors.forEach(function(error) {
                        // Find the closest form group and append the error
                        var errorSpan = $('<span class="text-danger"></span>').text(error);
                        $('.form-group').first().append(errorSpan);
                    });
                }
            }
        },
        error: function(xhr, status, error) {
            console.error('Error submitting form:', error);
            console.error('Status:', status);
            console.error('Response:', xhr.responseText);
            alert('Error updating project. Please try again.');
        }
    });
});

// Handle edit member button click
$(document).on('click', '[data-type="modal"][data-target="#edit-member-modal"]', function() {
    var memberId = $(this).closest('.member').data('member-id');
    
    // Show the modal first
    $('#edit-member-modal').addClass('modal-show');
    
    // Get member data
    $.ajax({
        url: '/admin/members/get/' + memberId,
        type: 'GET',
        success: function(result) {
            console.log('Received member data:', result);
            const form = $('#edit-member-form');
            
            // Set form values
            form.find('[name="Id"]').val(result.id);
            form.find('[name="MemberFirstName"]').val(result.memberFirstName);
            form.find('[name="MemberLastName"]').val(result.memberLastName);
            form.find('[name="MemberEmail"]').val(result.memberEmail);
            form.find('[name="MemberPhone"]').val(result.memberPhone);
            form.find('[name="MemberJobTitle"]').val(result.memberJobTitle);
            form.find('[name="MemberAddress"]').val(result.memberAddress);
            form.find('[name="MemberBirthDate"]').val(result.memberBirthDate);
            
            // Set image preview if exists
            if (result.memberImage) {
                const imagePreview = form.find('.image-preview-container img');
                imagePreview.attr('src', result.memberImage).removeClass('hide');
                form.find('.image-preview-container .circle').addClass('hide');
            }
        },
        error: function(xhr, status, error) {
            console.error('Error loading member data:', error);
            alert('Error loading member data. Please try again.');
        }
    });
});

// Edit member form submission handler
$(document).on('submit', '#edit-member-form', function(e) {
    e.preventDefault();
    
    // Clear any existing error messages
    $('.text-danger').text('');
    
    // Get the form data
    var formData = new FormData(this);
    
    // Log the form data for debugging
    console.log('Form data being sent:');
    for (var pair of formData.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
    }
    
    $.ajax({
        url: '/admin/members/edit/' + formData.get('Id'),
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(result) {
            if (result.success) {
                // Close the modal
                $('#edit-member-modal').removeClass('modal-show');
                // Reload the page to show the updated member
                location.reload();
            } else {
                // Display validation errors
                if (result.errors && result.errors.length > 0) {
                    result.errors.forEach(function(error) {
                        // Find the closest form group and append the error
                        var errorSpan = $('<span class="text-danger"></span>').text(error);
                        $('.form-group').first().append(errorSpan);
                    });
                }
            }
        },
        error: function(xhr, status, error) {
            console.error('Error submitting form:', error);
            console.error('Status:', status);
            console.error('Response:', xhr.responseText);
            alert('Error updating member. Please try again.');
        }
    });
});

// Handle edit client button click
$(document).on('click', '[data-type="modal"][data-target="#edit-client-modal"]', function() {
    var clientId = $(this).closest('.client').data('client-id');
    
    // Show the modal first
    $('#edit-client-modal').addClass('modal-show');
    
    // Get client data
    $.ajax({
        url: '/admin/clients/get/' + clientId,
        type: 'GET',
        success: function(result) {
            console.log('Received client data:', result);
            const form = $('#edit-client-form');
            
            // Set form values
            form.find('[name="Id"]').val(result.id);
            form.find('[name="ClientName"]').val(result.clientName);
            form.find('[name="ClientEmail"]').val(result.clientEmail);
            form.find('[name="ClientPhone"]').val(result.clientPhone);
            form.find('[name="ClientCompany"]').val(result.clientCompany);
            form.find('[name="ClientAddress"]').val(result.clientAddress);
            
            // Set image preview if exists
            if (result.clientImage) {
                const imagePreview = form.find('.image-preview-container img');
                imagePreview.attr('src', result.clientImage).removeClass('hide');
                form.find('.image-preview-container .circle').addClass('hide');
            }
        },
        error: function(xhr, status, error) {
            console.error('Error loading client data:', error);
            alert('Error loading client data. Please try again.');
        }
    });
});

// Edit client form submission handler
$(document).on('submit', '#edit-client-form', function(e) {
    e.preventDefault();
    
    // Clear any existing error messages
    $('.text-danger').text('');
    
    // Get the form data
    var formData = new FormData(this);
    
    // Log the form data for debugging
    console.log('Form data being sent:');
    for (var pair of formData.entries()) {
        console.log(pair[0] + ': ' + pair[1]);
    }
    
    $.ajax({
        url: '/admin/clients/edit/' + formData.get('Id'),
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function(result) {
            if (result.success) {
                // Close the modal
                $('#edit-client-modal').removeClass('modal-show');
                // Reload the page to show the updated client
                location.reload();
            } else {
                // Display validation errors
                if (result.errors && result.errors.length > 0) {
                    result.errors.forEach(function(error) {
                        // Find the closest form group and append the error
                        var errorSpan = $('<span class="text-danger"></span>').text(error);
                        $('.form-group').first().append(errorSpan);
                    });
                }
            }
        },
        error: function(xhr, status, error) {
            console.error('Error submitting form:', error);
            console.error('Status:', status);
            console.error('Response:', xhr.responseText);
            alert('Error updating client. Please try again.');
        }
    });
});

// Handle client delete
$(document).on('click', '.client .dropdown-action.remove', function(e) {
    e.preventDefault();
    var clientId = $(this).data('id');
    
    if (confirm('Are you sure you want to delete this client?')) {
        $.ajax({
            url: '/admin/clients/delete/' + clientId,
            type: 'POST',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function() {
                location.reload();
            },
            error: function(xhr, status, error) {
                console.error('Error deleting client:', error);
                alert('Error deleting client. Please try again.');
            }
        });
    }
});

// Handle member delete
$(document).on('click', '.member .dropdown-action.remove', function(e) {
    e.preventDefault();
    var memberId = $(this).data('id');
    
    if (confirm('Are you sure you want to delete this member?')) {
        $.ajax({
            url: '/admin/members/delete/' + memberId,
            type: 'POST',
            data: {
                __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
            },
            success: function() {
                location.reload();
            },
            error: function(xhr, status, error) {
                console.error('Error deleting member:', error);
                alert('Error deleting member. Please try again.');
            }
        });
    }
});
