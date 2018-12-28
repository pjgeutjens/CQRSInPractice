﻿using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Dtos;
using CSharpFunctionalExtensions;
using Logic.Students;
using Logic.Utils;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/students")]
    public sealed class StudentController : BaseController
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly Messages _messages;
        private readonly StudentRepository _studentRepository;
        private readonly CourseRepository _courseRepository;

        public StudentController(UnitOfWork unitOfWork, Messages messages)
        {
            _unitOfWork = unitOfWork;
            _messages = messages;
            _studentRepository = new StudentRepository(unitOfWork);
            _courseRepository = new CourseRepository(unitOfWork);
        }

        [HttpGet]
        public IActionResult GetList(string enrolled, int? number)
        {
            List<StudentDto> list = _messages.Dispatch(new GetListQuery(enrolled, number));
            return Ok(list);
        }

        private StudentDto ConvertToDto(Student student)
        {
            return new StudentDto
            {
                Id = student.Id,
                Name = student.Name,
                Email = student.Email,
                Course1 = student.FirstEnrollment?.Course?.Name,
                Course1Grade = student.FirstEnrollment?.Grade.ToString(),
                Course1Credits = student.FirstEnrollment?.Course?.Credits,
                Course2 = student.SecondEnrollment?.Course?.Name,
                Course2Grade = student.SecondEnrollment?.Grade.ToString(),
                Course2Credits = student.SecondEnrollment?.Course?.Credits,
            };
        }

        [HttpPost]
        public IActionResult Register([FromBody] NewStudentDto dto)
        {
            var student = new Student(dto.Name, dto.Email);

            if (dto.Course1 != null && dto.Course1Grade != null)
            {
                Course course = _courseRepository.GetByName(dto.Course1);
                student.Enroll(course, Enum.Parse<Grade>(dto.Course1Grade));
            }

            if (dto.Course2 != null && dto.Course2Grade != null)
            {
                Course course = _courseRepository.GetByName(dto.Course2);
                student.Enroll(course, Enum.Parse<Grade>(dto.Course2Grade));
            }

            _studentRepository.Save(student);
            _unitOfWork.Commit();

            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Unregister(long id)
        {
            Student student = _studentRepository.GetById(id);
            if (student == null)
                return Error($"No student found for Id {id}");

            _studentRepository.Delete(student);
            _unitOfWork.Commit();

            return Ok();
        }


        [HttpPost("{id}/enrollments")]
        public IActionResult Enroll(long id, [FromBody] StudentEnrollmentDto dto)
        {
            Student student = _studentRepository.GetById(id);
            if (student == null)
            {
                return Error($"No student with id '{id}'");
            }

            Course course = _courseRepository.GetByName(dto.Course);
            if (course == null)
            {
                return Error($"Course is incorrect: '{dto.Course}'");
            }

            bool success = Enum.TryParse(dto.Grade, out Grade grade);
            if (!success)
            {
                return Error($"Grade is incorrect: '{dto.Grade}'");
            }

            student.Enroll(course, grade);

            _unitOfWork.Commit();
            return Ok();

        }

        [HttpPut("{id}/enrollments/{enrollmentNumber}")]
        public IActionResult Transfer(long id, int enrollmentNumber, [FromBody] StudentTransferDto dto )
        {
            Student student = _studentRepository.GetById(id);
            if (student == null)
            {
                return Error($"No student with id '{id}'");
            }

            Course course = _courseRepository.GetByName(dto.Course);
            if (course == null)
            {
                return Error($"Course is incorrect: '{dto.Course}'");
            }

            bool success = Enum.TryParse(dto.Grade, out Grade grade);
            if (!success)
            {
                return Error($"Grade is incorrect: '{dto.Grade}'");
            }

            Enrollment enrollment = student.GetEnrollment(enrollmentNumber);
            if (enrollment == null)
            {
                return Error($"No enrollment found with number: '{enrollmentNumber}'");
            }

            enrollment.Update(course, grade);
            _unitOfWork.Commit();
            return Ok();

        }

        [HttpPost("{id}/enrollments/{enrollmentNumber}/deletion")]
        public IActionResult Disenroll(long id, int enrollmentNumber, [FromBody] StudentDisenrollmentDto dto)
        {
            Student student = _studentRepository.GetById(id);
            if (student == null)
            {
                return Error($"No student with id '{id}'");
            }

            if (string.IsNullOrWhiteSpace(dto.Comment))
                return Error("Disenrollment comment is required");

            Enrollment enrollment = student.GetEnrollment(enrollmentNumber);
            if (enrollment == null)
            {
                return Error($"No enrollment found with number: '{enrollmentNumber}'");
            }

            student.RemoveEnrollment(enrollment, dto.Comment);

            _unitOfWork.Commit();
            return Ok();

        }

        [HttpPut("{id}")]
        public IActionResult EditPersonalInfo(long id, [FromBody] StudentPersonalInfoDto dto )
        {
            var command = new EditPersonalInfoCommand(id, dto.Email, dto.Name);
           

            Result result = _messages.Dispatch(command);

            return result.IsSuccess ?  Ok() : Error(result.Error);
        }
    }
}