document.querySelectorAll('.form-select').forEach((select) => {
  const trigger = select.querySelector('.form-select-trigger')
  const triggerText = trigger.querySelector('.form-select-text')
  const options = select.querySelectorAll('.form-select-option')
  const hiddenInput = select.querySelector('input[type="hidden"]')
  const placeholder = select.dataset.placeholder || "Choose"

  const setValue = (value = "", text = placeholder) => {
    triggerText.textContent = text
    hiddenInput.value = value
    select.classList.toggle('has-placeholder', !value)
  };

  setValue()

  trigger.addEventListener('click', (e) => {
    e.stopPropagation();
    document
      .querySelectorAll(".form-select.open")
      .forEach((el) => el !== select && el.classList.remove('open'))
    select.classList.toggle('open')
  })

  options.forEach((option) =>
    option.addEventListener('click', () => {
      setValue(option.dataset.value, option.textContent)
      select.classList.remove('open')
    })
  )

  document.addEventListener('click', e => {
    if (!select.contains(e.target)) 
        select.classList.remove('open')
  })
})