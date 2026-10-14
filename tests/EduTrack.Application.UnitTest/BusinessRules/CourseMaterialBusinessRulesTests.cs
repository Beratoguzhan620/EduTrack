using Core.Exceptions;
using EduTrack.Application.BusinessRules;
using EduTrack.Application.DTOs.CourseMaterial;
using EduTrack.Domain.Entities;
using EduTrack.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Moq;

namespace EduTrack.Application.UnitTest.BusinessRules;

public class CourseMaterialBusinessRulesTests
{
    private readonly CourseMaterialBusinessRules _rules = new();

    private static CreateCourseMaterialRequest Request(MaterialType type, string? description = null, string? url = null, IFormFile? file = null) =>
        new() { Type = type, Title = "Kesirler", Description = description, Url = url, File = file };

    [Fact]
    public void Text_material_needs_content()
    {
        Assert.Throws<BusinessException>(() => _rules.DescriptionShouldNotBeEmptyWhenTypeIsContent(Request(MaterialType.Content, "  ")));
        _rules.DescriptionShouldNotBeEmptyWhenTypeIsContent(Request(MaterialType.Content, "Paydalar eşitlenir."));
        _rules.DescriptionShouldNotBeEmptyWhenTypeIsContent(Request(MaterialType.Link));
    }

    [Fact]
    public void Link_material_needs_an_address()
    {
        Assert.Throws<BusinessException>(() => _rules.UrlShouldNotBeEmptyWhenTypeIsLink(Request(MaterialType.Link)));
        _rules.UrlShouldNotBeEmptyWhenTypeIsLink(Request(MaterialType.Link, url: "https://example.com"));
    }

    [Theory]
    [InlineData("https://www.khanacademy.org/math")]
    [InlineData("http://example.com/odev.pdf")]
    public void Web_addresses_are_accepted(string url)
    {
        _rules.CheckLinkIsWebAddress(Request(MaterialType.Link, url: url));
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("ftp://example.com/file")]
    [InlineData("www.example.com")]
    public void Non_web_addresses_are_rejected(string url)
    {
        Assert.Throws<BusinessException>(() => _rules.CheckLinkIsWebAddress(Request(MaterialType.Link, url: url)));
    }

    [Fact]
    public void Document_material_needs_a_file()
    {
        Assert.Throws<BusinessException>(() => _rules.FileShouldNotBeNullWhenTypeIsDocument(Request(MaterialType.Document)));
        _rules.FileShouldNotBeNullWhenTypeIsDocument(Request(MaterialType.Document, file: Mock.Of<IFormFile>()));
    }

    [Fact]
    public void Only_the_instructor_adds_materials_and_only_members_read_them()
    {
        var instructorId = Guid.NewGuid();
        var course = new Course { InstructorId = instructorId };

        _rules.CheckIsCourseInstructor(course, instructorId);
        Assert.Throws<BusinessException>(() => _rules.CheckIsCourseInstructor(course, Guid.NewGuid()));

        _rules.CheckIsCourseMember(course, Guid.NewGuid(), isEnrolled: true);
        Assert.Throws<BusinessException>(() => _rules.CheckIsCourseMember(course, Guid.NewGuid(), isEnrolled: false));
    }
}
