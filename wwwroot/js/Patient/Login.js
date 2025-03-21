$(document).ready(function () {
  // Initialize the form
  $("#PatientLogin").kendoForm({
    validatable: {
      validateOnBlur: true,
      errorTemplate: "<span class='text text-danger'>#=message#</span>",
    },
    formData: {
      c_email: "",
      c_password: "",
    },
    items: [
      {
        field: "c_email",
        label: "Email",
        validation: {
          required: { message: "Please enter email." },
          customEmailValidation: function (input) {
            if (input.is("[name='c_email']")) {
              var emailPattern =
                /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
              if (!emailPattern.test(input.val())) {
                input.attr(
                  "data-customEmailValidation-msg",
                  "Please enter a valid email address."
                );
                return false;
              }
            }
            return true;
          },
        },
      },
      {
        field: "c_password",
        label: "Password",
        editor: function (container, options) {
          $(
            '<input type="password" id="c_password" name="' +
              options.field +
              '" title="Password" autocomplete="off" aria-labelledby="Password-form-label" data-bind="value: Password" aria-describedby="Password-form-hint"/>'
          )
            .appendTo(container)
            .kendoTextBox();
        },
        validation: {
          required: { message: "Please enter your password." },
          pwdLength: function (input) {
            if (input.is("[name='c_password']") && input.val().length < 8) {
              input.attr(
                "data-pwdLength-msg",
                "Password Has minimum 8 characters in length."
              );
              return false;
            }
            if (input.is("[name='c_password']")) {
              var pwdPattern = /^(?=.*?[A-Z]).{8,}$/;
              if (!pwdPattern.test(input.val())) {
                input.attr(
                  "data-pwdLength-msg",
                  "Please enter At least one uppercase English letter."
                );
                return false;
              }
            }
            if (input.is("[name='c_password']")) {
              var pwdPattern = /^(?=.*?[a-z]).{8,}$/;
              if (!pwdPattern.test(input.val())) {
                input.attr(
                  "data-pwdLength-msg",
                  "Please enter At least one lowercase English letter."
                );
                return false;
              }
            }
            if (input.is("[name='c_password']")) {
              var pwdPattern = /^(?=.*?[0-9]).{8,}$/;
              if (!pwdPattern.test(input.val())) {
                input.attr(
                  "data-pwdLength-msg",
                  "Please enter At least one digit."
                );
                return false;
              }
            }
            if (input.is("[name='c_password']")) {
              var pwdPattern = /^(?=.*?[#?!@$%^&*-]).{8,}$/;
              if (!pwdPattern.test(input.val())) {
                input.attr(
                  "data-pwdLength-msg",
                  "Please enter At least one special character."
                );
                return false;
              }
            }
            return true;
          },
        },
      },
    ],
  });
});
