using System.Collections.Generic;
using Logic.Dtos;
using CSharpFunctionalExtensions;
using Logic.Students;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/students")]
    public sealed class StudentController : BaseController
    {
        private readonly Messages _messages;


        public StudentController(Messages messages)
        {
            _messages = messages;
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
            var command = new RegisterCommand(
                dto.Name, dto.Email,
                dto.Course1, dto.Course1Grade,
                dto.Course2, dto.Course2Grade);

            Result result = _messages.Dispatch(command);

            return FromResult(result);
        }

        [HttpDelete("{id}")]
        public IActionResult Unregister(long id)
        {
            Result result = _messages.Dispatch(new UnregisterCommand(id));
            return FromResult(result);

        }


        [HttpPost("{id}/enrollments")]
        public IActionResult Enroll(long id, [FromBody] StudentEnrollmentDto dto)
        {
            var command = new EnrollCommand(
                    id, dto.Course, dto.Grade
                );

            Result result = _messages.Dispatch(command);
            return FromResult(result);
        }

        [HttpPut("{id}/enrollments/{enrollmentNumber}")]
        public IActionResult Transfer(long id, int enrollmentNumber, [FromBody] StudentTransferDto dto )
        {
            var command = new TransferCommand(
                id, enrollmentNumber, dto.Course, dto.Grade);
            Result result = _messages.Dispatch(command);

            return FromResult(result);
        }

        [HttpPost("{id}/enrollments/{enrollmentNumber}/deletion")]
        public IActionResult Disenroll(long id, int enrollmentNumber, [FromBody] StudentDisenrollmentDto dto)
        {
            var command = new DisenrollCommand(id, enrollmentNumber, dto.Comment);
            Result result = _messages.Dispatch(command);

            return FromResult(result);

        }

        [HttpPut("{id}")]
        public IActionResult EditPersonalInfo(long id, [FromBody] StudentPersonalInfoDto dto )
        {
            var command = new EditPersonalInfoCommand(id, dto.Name, dto.Email);
           

            Result result = _messages.Dispatch(command);

            return FromResult(result);
        }
    }
}
