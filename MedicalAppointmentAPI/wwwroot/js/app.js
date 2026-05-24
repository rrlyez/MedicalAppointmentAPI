const api = {
    patients: '/api/Patients',
    specializations: '/api/Specializations',
    doctors: '/api/Doctors',
    schedules: '/api/Schedules',
    appointments: '/api/Appointments'
};

let patients = [];
let specializations = [];
let doctors = [];
let schedules = [];
let appointments = [];

const dayNames = [
    'Sunday',
    'Monday',
    'Tuesday',
    'Wednesday',
    'Thursday',
    'Friday',
    'Saturday'
];

document.addEventListener('DOMContentLoaded', loadAllData);

function showSection(sectionId) {
    document.querySelectorAll('.section').forEach(section => {
        section.classList.remove('active');
    });

    document.getElementById(sectionId).classList.add('active');
}

function showMessage(text) {
    document.getElementById('message').textContent = text.length > 150 ? 'Error while saving data.' : text;
    setTimeout(() => {
        document.getElementById('message').textContent = '';
    }, 3000);
}

async function loadAllData() {
    try {
        patients = await getData(api.patients);
        specializations = await getData(api.specializations);
        doctors = await getData(api.doctors);
        schedules = await getData(api.schedules);
        appointments = await getData(api.appointments);

        fillSelects();

        renderPatients();
        renderSpecializations();
        renderDoctors();
        renderSchedules();
        renderAppointments();
    } catch (error) {
        showMessage('Error loading data.');
        console.error(error);
    }
}

async function getData(url) {
    const response = await fetch(url);

    if (!response.ok) {
        throw new Error('Request error');
    }

    return await response.json();
}

async function sendData(url, method, data) {
    const response = await fetch(url, {
        method: method,
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || 'Request error');
    }

    if (response.status === 204) {
        return null;
    }

    return await response.json();
}

async function deleteData(url) {
    const response = await fetch(url, {
        method: 'DELETE'
    });

    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || 'Delete error');
    }
}

function fillSelects() {
    fillSpecializationSelects();
    fillDoctorSelects();
    fillPatientSelects();
}

function fillSpecializationSelects() {
    const doctorSpecializationSelect = document.getElementById('doctorSpecializationId');

    doctorSpecializationSelect.innerHTML = '';

    specializations.forEach(specialization => {
        const option = document.createElement('option');
        option.value = specialization.specializationId;
        option.textContent = specialization.name;
        doctorSpecializationSelect.appendChild(option);
    });
}

function fillDoctorSelects() {
    const scheduleDoctorSelect = document.getElementById('scheduleDoctorId');
    const appointmentDoctorSelect = document.getElementById('appointmentDoctorId');

    scheduleDoctorSelect.innerHTML = '';
    appointmentDoctorSelect.innerHTML = '';

    doctors.forEach(doctor => {
        const option1 = document.createElement('option');
        option1.value = doctor.doctorId;
        option1.textContent = doctor.fullName;
        scheduleDoctorSelect.appendChild(option1);

        const option2 = document.createElement('option');
        option2.value = doctor.doctorId;
        option2.textContent = doctor.fullName;
        appointmentDoctorSelect.appendChild(option2);
    });
}

function fillPatientSelects() {
    const appointmentPatientSelect = document.getElementById('appointmentPatientId');

    appointmentPatientSelect.innerHTML = '';

    patients.forEach(patient => {
        const option = document.createElement('option');
        option.value = patient.patientId;
        option.textContent = patient.fullName;
        appointmentPatientSelect.appendChild(option);
    });
}

function getPatientName(id) {
    const patient = patients.find(p => p.patientId === id);
    return patient ? patient.fullName : id;
}

function getDoctorName(id) {
    const doctor = doctors.find(d => d.doctorId === id);
    return doctor ? doctor.fullName : id;
}

function getSpecializationName(id) {
    const specialization = specializations.find(s => s.specializationId === id);
    return specialization ? specialization.name : id;
}

function formatTime(time) {
    if (!time) {
        return '';
    }

    return time.substring(0, 5);
}

function formatDateTime(dateTime) {
    if (!dateTime) {
        return '';
    }

    return new Date(dateTime).toLocaleString('uk-UA');
}

function toDateTimeLocalValue(dateTime) {
    if (!dateTime) {
        return '';
    }

    const date = new Date(dateTime);
    const offset = date.getTimezoneOffset();
    const localDate = new Date(date.getTime() - offset * 60000);

    return localDate.toISOString().substring(0, 16);
}

/* PATIENTS */

function renderPatients() {
    const table = document.getElementById('patientsTable');
    table.innerHTML = '';

    patients.forEach(patient => {
        table.innerHTML += `
            <tr>
                <td>${patient.patientId}</td>
                <td>${patient.fullName}</td>
                <td>${patient.phone}</td>
                <td>${patient.email ?? ''}</td>
                <td>
                    <button class="edit" onclick="editPatient(${patient.patientId})">Edit</button>
                    <button class="delete" onclick="deletePatient(${patient.patientId})">Delete</button>
                </td>
            </tr>
        `;
    });
}

async function savePatient(event) {
    event.preventDefault();

    const id = document.getElementById('patientId').value;

    const patient = {
        patientId: id ? Number(id) : 0,
        fullName: document.getElementById('patientFullName').value,
        phone: document.getElementById('patientPhone').value,
        email: document.getElementById('patientEmail').value || null
    };

    try {
        if (id) {
            await sendData(`${api.patients}/${id}`, 'PUT', patient);
            showMessage('Patient updated.');
        } else {
            await sendData(api.patients, 'POST', patient);
            showMessage('Patient added.');
        }

        clearPatientForm();
        await loadAllData();
    } catch (error) {
        showMessage(error.message);
    }
}

function editPatient(id) {
    const patient = patients.find(p => p.patientId === id);

    document.getElementById('patientId').value = patient.patientId;
    document.getElementById('patientFullName').value = patient.fullName;
    document.getElementById('patientPhone').value = patient.phone;
    document.getElementById('patientEmail').value = patient.email ?? '';
}

async function deletePatient(id) {
    if (!confirm('Delete this patient?')) {
        return;
    }

    try {
        await deleteData(`${api.patients}/${id}`);
        showMessage('Patient deleted.');
        await loadAllData();
    } catch (error) {
        showMessage(error.message);
    }
}

function clearPatientForm() {
    document.getElementById('patientId').value = '';
    document.getElementById('patientFullName').value = '';
    document.getElementById('patientPhone').value = '';
    document.getElementById('patientEmail').value = '';
}

/* SPECIALIZATIONS */

function renderSpecializations() {
    const table = document.getElementById('specializationsTable');
    table.innerHTML = '';

    specializations.forEach(specialization => {
        table.innerHTML += `
            <tr>
                <td>${specialization.specializationId}</td>
                <td>${specialization.name}</td>
                <td>
                    <button class="edit" onclick="editSpecialization(${specialization.specializationId})">Edit</button>
                    <button class="delete" onclick="deleteSpecialization(${specialization.specializationId})">Delete</button>
                </td>
            </tr>
        `;
    });
}

async function saveSpecialization(event) {
    event.preventDefault();

    const id = document.getElementById('specializationId').value;

    const specialization = {
        specializationId: id ? Number(id) : 0,
        name: document.getElementById('specializationName').value
    };

    try {
        if (id) {
            await sendData(`${api.specializations}/${id}`, 'PUT', specialization);
            showMessage('Specialization updated.');
        } else {
            await sendData(api.specializations, 'POST', specialization);
            showMessage('Specialization added.');
        }

        clearSpecializationForm();
        await loadAllData();
    } catch (error) {
        showMessage(error.message);
    }
}

function editSpecialization(id) {
    const specialization = specializations.find(s => s.specializationId === id);

    document.getElementById('specializationId').value = specialization.specializationId;
    document.getElementById('specializationName').value = specialization.name;
}

async function deleteSpecialization(id) {
    if (!confirm('Delete this specialization?')) {
        return;
    }

    try {
        await deleteData(`${api.specializations}/${id}`);
        showMessage('Specialization deleted.');
        await loadAllData();
    } catch (error) {
        showMessage(error.message);
    }
}

function clearSpecializationForm() {
    document.getElementById('specializationId').value = '';
    document.getElementById('specializationName').value = '';
}

/* DOCTORS */

function renderDoctors() {
    const table = document.getElementById('doctorsTable');
    table.innerHTML = '';

    doctors.forEach(doctor => {
        table.innerHTML += `
            <tr>
                <td>${doctor.doctorId}</td>
                <td>${doctor.fullName}</td>
                <td>${getSpecializationName(doctor.specializationId)}</td>
                <td>
                    <button class="edit" onclick="editDoctor(${doctor.doctorId})">Edit</button>
                    <button class="delete" onclick="deleteDoctor(${doctor.doctorId})">Delete</button>
                </td>
            </tr>
        `;
    });
}

async function saveDoctor(event) {
    event.preventDefault();

    const id = document.getElementById('doctorId').value;

    const doctor = {
        doctorId: id ? Number(id) : 0,
        fullName: document.getElementById('doctorFullName').value,
        specializationId: Number(document.getElementById('doctorSpecializationId').value)
    };

    try {
        if (id) {
            await sendData(`${api.doctors}/${id}`, 'PUT', doctor);
            showMessage('Doctor updated.');
        } else {
            await sendData(api.doctors, 'POST', doctor);
            showMessage('Doctor added.');
        }

        clearDoctorForm();
        await loadAllData();
    } catch (error) {
        showMessage(error.message);
    }
}

function editDoctor(id) {
    const doctor = doctors.find(d => d.doctorId === id);

    document.getElementById('doctorId').value = doctor.doctorId;
    document.getElementById('doctorFullName').value = doctor.fullName;
    document.getElementById('doctorSpecializationId').value = doctor.specializationId;
}

async function deleteDoctor(id) {
    if (!confirm('Delete this doctor?')) {
        return;
    }

    try {
        await deleteData(`${api.doctors}/${id}`);
        showMessage('Doctor deleted.');
        await loadAllData();
    } catch (error) {
        showMessage(error.message);
    }
}

function clearDoctorForm() {
    document.getElementById('doctorId').value = '';
    document.getElementById('doctorFullName').value = '';
}

/* SCHEDULES */

function renderSchedules() {
    const table = document.getElementById('schedulesTable');
    table.innerHTML = '';

    schedules.forEach(schedule => {
        table.innerHTML += `
            <tr>
                <td>${schedule.scheduleId}</td>
                <td>${getDoctorName(schedule.doctorId)}</td>
                <td>${dayNames[schedule.dayOfWeek]}</td>
                <td>${formatTime(schedule.startTime)}</td>
                <td>${formatTime(schedule.endTime)}</td>
                <td>
                    <button class="edit" onclick="editSchedule(${schedule.scheduleId})">Edit</button>
                    <button class="delete" onclick="deleteSchedule(${schedule.scheduleId})">Delete</button>
                </td>
            </tr>
        `;
    });
}

async function saveSchedule(event) {
    event.preventDefault();

    const id = document.getElementById('scheduleId').value;

    const schedule = {
        scheduleId: id ? Number(id) : 0,
        doctorId: Number(document.getElementById('scheduleDoctorId').value),
        dayOfWeek: Number(document.getElementById('scheduleDayOfWeek').value),
        startTime: document.getElementById('scheduleStartTime').value + ':00',
        endTime: document.getElementById('scheduleEndTime').value + ':00'
    };

    try {
        if (id) {
            await sendData(`${api.schedules}/${id}`, 'PUT', schedule);
            showMessage('Schedule updated.');
        } else {
            await sendData(api.schedules, 'POST', schedule);
            showMessage('Schedule added.');
        }

        clearScheduleForm();
        await loadAllData();
    } catch (error) {
        showMessage(error.message);
    }
}

function editSchedule(id) {
    const schedule = schedules.find(s => s.scheduleId === id);

    document.getElementById('scheduleId').value = schedule.scheduleId;
    document.getElementById('scheduleDoctorId').value = schedule.doctorId;
    document.getElementById('scheduleDayOfWeek').value = schedule.dayOfWeek;
    document.getElementById('scheduleStartTime').value = formatTime(schedule.startTime);
    document.getElementById('scheduleEndTime').value = formatTime(schedule.endTime);
}

async function deleteSchedule(id) {
    if (!confirm('Delete this schedule?')) {
        return;
    }

    try {
        await deleteData(`${api.schedules}/${id}`);
        showMessage('Schedule deleted.');
        await loadAllData();
    } catch (error) {
        showMessage(error.message);
    }
}

function clearScheduleForm() {
    document.getElementById('scheduleId').value = '';
    document.getElementById('scheduleStartTime').value = '';
    document.getElementById('scheduleEndTime').value = '';
}

/* APPOINTMENTS */

function renderAppointments() {
    const table = document.getElementById('appointmentsTable');
    table.innerHTML = '';

    appointments.forEach(appointment => {
        table.innerHTML += `
            <tr>
                <td>${appointment.appointmentId}</td>
                <td>${getPatientName(appointment.patientId)}</td>
                <td>${getDoctorName(appointment.doctorId)}</td>
                <td>${formatDateTime(appointment.appointmentDateTime)}</td>
                <td>${appointment.durationMinutes} min</td>
                <td>${appointment.status}</td>
                <td>
                    <button class="edit" onclick="editAppointment(${appointment.appointmentId})">Edit</button>
                    <button class="delete" onclick="deleteAppointment(${appointment.appointmentId})">Delete</button>
                </td>
            </tr>
        `;
    });
}

async function saveAppointment(event) {
    event.preventDefault();

    const id = document.getElementById('appointmentId').value;

    const localDateTime = document.getElementById('appointmentDateTime').value;

    const appointment = {
        appointmentId: id ? Number(id) : 0,
        patientId: Number(document.getElementById('appointmentPatientId').value),
        doctorId: Number(document.getElementById('appointmentDoctorId').value),
        appointmentDateTime: new Date(localDateTime).toISOString(),
        durationMinutes: Number(document.getElementById('appointmentDurationMinutes').value),
        status: document.getElementById('appointmentStatus').value
    };

    try {
        if (id) {
            await sendData(`${api.appointments}/${id}`, 'PUT', appointment);
            showMessage('Appointment updated.');
        } else {
            await sendData(api.appointments, 'POST', appointment);
            showMessage('Appointment added.');
        }

        clearAppointmentForm();
        await loadAllData();
    } catch (error) {
        showMessage(error.message);
    }
}

function editAppointment(id) {
    const appointment = appointments.find(a => a.appointmentId === id);

    document.getElementById('appointmentId').value = appointment.appointmentId;
    document.getElementById('appointmentPatientId').value = appointment.patientId;
    document.getElementById('appointmentDoctorId').value = appointment.doctorId;
    document.getElementById('appointmentDateTime').value = toDateTimeLocalValue(appointment.appointmentDateTime);
    document.getElementById('appointmentDurationMinutes').value = appointment.durationMinutes;
    document.getElementById('appointmentStatus').value = appointment.status;
}

async function deleteAppointment(id) {
    if (!confirm('Delete this appointment?')) {
        return;
    }

    try {
        await deleteData(`${api.appointments}/${id}`);
        showMessage('Appointment deleted.');
        await loadAllData();
    } catch (error) {
        showMessage(error.message);
    }
}

function clearAppointmentForm() {
    document.getElementById('appointmentId').value = '';
    document.getElementById('appointmentDateTime').value = '';
    document.getElementById('appointmentDurationMinutes').value = 30;
    document.getElementById('appointmentStatus').value = 'Scheduled';
}