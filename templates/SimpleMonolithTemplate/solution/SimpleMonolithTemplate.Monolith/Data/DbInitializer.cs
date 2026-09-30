using Microsoft.EntityFrameworkCore;
using SimpleMonolithTemplate.Monolith.Data.Entities.ContosoUniversity;
using SimpleMonolithTemplate.Monolith.Data.Persistence;

namespace SimpleMonolithTemplate.Monolith.Data;

public static class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<StartUp>>();
        var dbContext = serviceProvider.GetRequiredService<SimpleMonolithTemplateDbContext>();

        try
        {
            // Apply pending migrations
            await dbContext.Database.MigrateAsync();

            // Prevent duplicate seed execution
            if (await dbContext.TestStudents.AnyAsync())
            {
                return;
            }

            // Seed the database with initial data
            await SeedDB(dbContext);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database migration/seeding");
            throw;
        }
    }

    private static async Task SeedDB(SimpleMonolithTemplateDbContext dbContext)
    {
        // === CONTOSO UNIVERSITY SEEDING FOR CRUD EXAMPLE
        IEnumerable<TestStudent> students = GetTestStudents();
        dbContext.TestStudents.AddRange(students);
        dbContext.SaveChanges();

        IEnumerable<TestInstructor> instructors = GetTestInstructors();
        dbContext.TestInstructors.AddRange(instructors);
        dbContext.SaveChanges();

        IEnumerable<TestDepartment> departments = GetTestDepartments();
        dbContext.TestDepartments.AddRange(departments);
        dbContext.SaveChanges();

        IEnumerable<TestCourse> courses = GetTestCourses(departments);
        dbContext.TestCourses.AddRange(courses);
        dbContext.SaveChanges();

        IEnumerable<TestOfficeAssignment> officeAssignments = GetTestOfficeAssignments(instructors);
        dbContext.TestOfficeAssignments.AddRange(officeAssignments);
        dbContext.SaveChanges();

        IEnumerable<TestCourseAssignment> courseTestInstructors = GetTestCourseAssignments(
            instructors,
            courses
        );
        dbContext.TestCourseAssignments.AddRange(courseTestInstructors);
        dbContext.SaveChanges();

        IEnumerable<TestEnrollment> enrollments = GetTestEnrollments(students, courses);
        foreach (TestEnrollment e in enrollments)
        {
            var enrollmentInDataBase = dbContext
                .TestEnrollments.Where(s =>
                    s.TestStudent.TestStudentId == e.TestStudentId
                    && s.TestCourse.TestCourseId == e.TestCourseId
                )
                .SingleOrDefault();
            if (enrollmentInDataBase == null)
            {
                dbContext.TestEnrollments.Add(e);
            }
        }
        dbContext.SaveChanges();
    }

    private static IEnumerable<TestCourseAssignment> GetTestCourseAssignments(
        IEnumerable<TestInstructor> instructors,
        IEnumerable<TestCourse> courses
    )
    {
        return
        [
            new TestCourseAssignment
            {
                TestCourseId = courses.Single(c => c.Title == "Chemistry").TestCourseId,
                TestInstructorId = instructors.Single(i => i.LastName == "Kapoor").TestInstructorId,
            },
            new TestCourseAssignment
            {
                TestCourseId = courses.Single(c => c.Title == "Chemistry").TestCourseId,
                TestInstructorId = instructors.Single(i => i.LastName == "Harui").TestInstructorId,
            },
            new TestCourseAssignment
            {
                TestCourseId = courses.Single(c => c.Title == "Microeconomics").TestCourseId,
                TestInstructorId = instructors.Single(i => i.LastName == "Zheng").TestInstructorId,
            },
            new TestCourseAssignment
            {
                TestCourseId = courses.Single(c => c.Title == "Macroeconomics").TestCourseId,
                TestInstructorId = instructors.Single(i => i.LastName == "Zheng").TestInstructorId,
            },
            new TestCourseAssignment
            {
                TestCourseId = courses.Single(c => c.Title == "Calculus").TestCourseId,
                TestInstructorId = instructors
                    .Single(i => i.LastName == "Fakhouri")
                    .TestInstructorId,
            },
            new TestCourseAssignment
            {
                TestCourseId = courses.Single(c => c.Title == "Trigonometry").TestCourseId,
                TestInstructorId = instructors.Single(i => i.LastName == "Harui").TestInstructorId,
            },
            new TestCourseAssignment
            {
                TestCourseId = courses.Single(c => c.Title == "Composition").TestCourseId,
                TestInstructorId = instructors
                    .Single(i => i.LastName == "Abercrombie")
                    .TestInstructorId,
            },
            new TestCourseAssignment
            {
                TestCourseId = courses.Single(c => c.Title == "Literature").TestCourseId,
                TestInstructorId = instructors
                    .Single(i => i.LastName == "Abercrombie")
                    .TestInstructorId,
            },
        ];
    }

    private static IEnumerable<TestOfficeAssignment> GetTestOfficeAssignments(
        IEnumerable<TestInstructor> instructors
    )
    {
        return
        [
            new TestOfficeAssignment
            {
                TestInstructorId = instructors
                    .Single(i => i.LastName == "Fakhouri")
                    .TestInstructorId,
                Location = "Smith 17",
            },
            new TestOfficeAssignment
            {
                TestInstructorId = instructors.Single(i => i.LastName == "Harui").TestInstructorId,
                Location = "Gowan 27",
            },
            new TestOfficeAssignment
            {
                TestInstructorId = instructors.Single(i => i.LastName == "Kapoor").TestInstructorId,
                Location = "Thompson 304",
            },
        ];
    }

    private static IEnumerable<TestDepartment> GetTestDepartments()
    {
        return
        [
            new TestDepartment
            {
                Name = "Computer Science",
                Budget = 350000,
                StartDate = DateTime.Parse("2007-09-01"),
                TestInstructorId = 1,
            },
            new TestDepartment
            {
                Name = "Mathematics",
                Budget = 100000,
                StartDate = DateTime.Parse("2007-09-01"),
                TestInstructorId = 2,
            },
            new TestDepartment
            {
                Name = "English",
                Budget = 200000,
                StartDate = DateTime.Parse("2007-09-01"),
                TestInstructorId = 3,
            },
            new TestDepartment
            {
                Name = "Engineering",
                Budget = 300000,
                StartDate = DateTime.Parse("2007-09-01"),
                TestInstructorId = 4,
            },
            new TestDepartment
            {
                Name = "Economics",
                Budget = 250000,
                StartDate = DateTime.Parse("2007-09-01"),
                TestInstructorId = 5,
            },
        ];
    }

    private static IEnumerable<TestInstructor> GetTestInstructors()
    {
        return
        [
            new TestInstructor
            {
                FirstMidName = "Kim",
                LastName = "Abercrombie",
                HireDate = DateTime.Parse("1995-03-11"),
            },
            new TestInstructor
            {
                FirstMidName = "Fadi",
                LastName = "Fakhouri",
                HireDate = DateTime.Parse("2002-07-06"),
            },
            new TestInstructor
            {
                FirstMidName = "Roger",
                LastName = "Harui",
                HireDate = DateTime.Parse("1998-07-01"),
            },
            new TestInstructor
            {
                FirstMidName = "Candace",
                LastName = "Kapoor",
                HireDate = DateTime.Parse("2001-01-15"),
            },
            new TestInstructor
            {
                FirstMidName = "Roger",
                LastName = "Zheng",
                HireDate = DateTime.Parse("2004-02-12"),
            },
        ];
    }

    private static IEnumerable<TestEnrollment> GetTestEnrollments(
        IEnumerable<TestStudent> students,
        IEnumerable<TestCourse> courses
    )
    {
        return
        [
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Alexander").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Chemistry").TestCourseId,
                Grade = Grade.A,
            },
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Alexander").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Microeconomics").TestCourseId,
                Grade = Grade.C,
            },
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Alexander").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Macroeconomics").TestCourseId,
                Grade = Grade.B,
            },
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Alonso").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Calculus").TestCourseId,
                Grade = Grade.B,
            },
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Alonso").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Trigonometry").TestCourseId,
                Grade = Grade.B,
            },
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Alonso").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Composition").TestCourseId,
                Grade = Grade.B,
            },
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Anand").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Chemistry").TestCourseId,
            },
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Anand").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Microeconomics").TestCourseId,
                Grade = Grade.B,
            },
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Barzdukas").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Chemistry").TestCourseId,
                Grade = Grade.B,
            },
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Li").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Composition").TestCourseId,
                Grade = Grade.B,
            },
            new TestEnrollment
            {
                TestStudentId = students.Single(s => s.LastName == "Justice").TestStudentId,
                TestCourseId = courses.Single(c => c.Title == "Literature").TestCourseId,
                Grade = Grade.B,
            },
        ];
    }

    private static IEnumerable<TestCourse> GetTestCourses(IEnumerable<TestDepartment> departments)
    {
        return
        [
            new TestCourse
            {
                TestCourseId = 1050,
                Title = "Chemistry",
                Credits = 3,
                TestDepartmentId = departments
                    .Single(s => s.Name == "Engineering")
                    .TestDepartmentId,
            },
            new TestCourse
            {
                TestCourseId = 4022,
                Title = "Microeconomics",
                Credits = 3,
                TestDepartmentId = departments.Single(s => s.Name == "Economics").TestDepartmentId,
            },
            new TestCourse
            {
                TestCourseId = 4041,
                Title = "Macroeconomics",
                Credits = 3,
                TestDepartmentId = departments.Single(s => s.Name == "Economics").TestDepartmentId,
            },
            new TestCourse
            {
                TestCourseId = 1045,
                Title = "Calculus",
                Credits = 4,
                TestDepartmentId = departments
                    .Single(s => s.Name == "Mathematics")
                    .TestDepartmentId,
            },
            new TestCourse
            {
                TestCourseId = 3141,
                Title = "Trigonometry",
                Credits = 4,
                TestDepartmentId = departments
                    .Single(s => s.Name == "Mathematics")
                    .TestDepartmentId,
            },
            new TestCourse
            {
                TestCourseId = 2021,
                Title = "Composition",
                Credits = 3,
                TestDepartmentId = departments.Single(s => s.Name == "English").TestDepartmentId,
            },
            new TestCourse
            {
                TestCourseId = 2042,
                Title = "Literature",
                Credits = 4,
                TestDepartmentId = departments.Single(s => s.Name == "English").TestDepartmentId,
            },
        ];
    }

    private static IEnumerable<TestStudent> GetTestStudents()
    {
        return
        [
            new TestStudent
            {
                GovernmentId = "45892341",
                FirstMidName = "Carson",
                LastName = "Alexander",
                TestEnrollmentDate = DateOnly.Parse("1999-09-01"),
            },
            new TestStudent
            {
                GovernmentId = "52173456",
                FirstMidName = "Meredith",
                LastName = "Alonso",
                TestEnrollmentDate = DateOnly.Parse("2021-09-01"),
            },
            new TestStudent
            {
                GovernmentId = "48765234",
                FirstMidName = "Arturo",
                LastName = "Anand",
                TestEnrollmentDate = DateOnly.Parse("2003-09-01"),
            },
            new TestStudent
            {
                GovernmentId = "41234567",
                FirstMidName = "Gytis",
                LastName = "Barzdukas",
                TestEnrollmentDate = DateOnly.Parse("1999-09-01"),
            },
            new TestStudent
            {
                GovernmentId = "59876543",
                FirstMidName = "Yan",
                LastName = "Li",
                TestEnrollmentDate = DateOnly.Parse("2002-09-01"),
            },
            new TestStudent
            {
                GovernmentId = "43567890",
                FirstMidName = "Peggy",
                LastName = "Justice",
                TestEnrollmentDate = DateOnly.Parse("2001-09-01"),
            },
            new TestStudent
            {
                GovernmentId = "56789012",
                FirstMidName = "Laura",
                LastName = "Norman",
                TestEnrollmentDate = DateOnly.Parse("2003-09-01"),
            },
            new TestStudent
            {
                GovernmentId = "47890123",
                FirstMidName = "Nino",
                LastName = "Olivetto",
                TestEnrollmentDate = DateOnly.Parse("2010-09-01"),
            },
            new TestStudent
            {
                GovernmentId = "51234568",
                FirstMidName = "Brandon",
                LastName = "Mitchell",
                TestEnrollmentDate = DateOnly.Parse("1995-03-15"),
            },
            new TestStudent
            {
                GovernmentId = "44567891",
                FirstMidName = "Emily",
                LastName = "Johnson",
                TestEnrollmentDate = DateOnly.Parse("2018-01-22"),
            },
            new TestStudent
            {
                GovernmentId = "58901234",
                FirstMidName = "Michael",
                LastName = "Davis",
                TestEnrollmentDate = DateOnly.Parse("2007-06-10"),
            },
            new TestStudent
            {
                GovernmentId = "42345679",
                FirstMidName = "Sarah",
                LastName = "Wilson",
                TestEnrollmentDate = DateOnly.Parse("2012-11-03"),
            },
            new TestStudent
            {
                GovernmentId = "55678902",
                FirstMidName = "Joshua",
                LastName = "Martinez",
                TestEnrollmentDate = DateOnly.Parse("1998-08-17"),
            },
            new TestStudent
            {
                GovernmentId = "49012345",
                FirstMidName = "Ashley",
                LastName = "Anderson",
                TestEnrollmentDate = DateOnly.Parse("2015-04-29"),
            },
            new TestStudent
            {
                GovernmentId = "53456780",
                FirstMidName = "Christop",
                LastName = "Taylor",
                TestEnrollmentDate = DateOnly.Parse("2001-12-08"),
            },
            new TestStudent
            {
                GovernmentId = "46789013",
                FirstMidName = "Jessica",
                LastName = "Thomas",
                TestEnrollmentDate = DateOnly.Parse("2019-07-14"),
            },
            new TestStudent
            {
                GovernmentId = "40123456",
                FirstMidName = "Matthew",
                LastName = "Jackson",
                TestEnrollmentDate = DateOnly.Parse("2005-02-21"),
            },
            new TestStudent
            {
                GovernmentId = "54567892",
                FirstMidName = "Amanda",
                LastName = "White",
                TestEnrollmentDate = DateOnly.Parse("2013-10-06"),
            },
            new TestStudent
            {
                GovernmentId = "47890124",
                FirstMidName = "Daniel",
                LastName = "Harris",
                TestEnrollmentDate = DateOnly.Parse("1997-05-19"),
            },
            new TestStudent
            {
                GovernmentId = "51234569",
                FirstMidName = "Stephan",
                LastName = "Martin",
                TestEnrollmentDate = DateOnly.Parse("2020-09-27"),
            },
            new TestStudent
            {
                GovernmentId = "45678903",
                FirstMidName = "DavId",
                LastName = "Thompson",
                TestEnrollmentDate = DateOnly.Parse("2008-03-12"),
            },
            new TestStudent
            {
                GovernmentId = "59012346",
                FirstMidName = "Jennifer",
                LastName = "Garcia",
                TestEnrollmentDate = DateOnly.Parse("2016-11-25"),
            },
            new TestStudent
            {
                GovernmentId = "43456781",
                FirstMidName = "Andrew",
                LastName = "Martinez",
                TestEnrollmentDate = DateOnly.Parse("2002-08-03"),
            },
            new TestStudent
            {
                GovernmentId = "56789014",
                FirstMidName = "Nicole",
                LastName = "Robinson",
                TestEnrollmentDate = DateOnly.Parse("2011-01-16"),
            },
            new TestStudent
            {
                GovernmentId = "50123457",
                FirstMidName = "Ryan",
                LastName = "Clark",
                TestEnrollmentDate = DateOnly.Parse("1999-06-30"),
            },
            new TestStudent
            {
                GovernmentId = "44567893",
                FirstMidName = "Samantha",
                LastName = "Rodriguez",
                TestEnrollmentDate = DateOnly.Parse("2017-04-08"),
            },
            new TestStudent
            {
                GovernmentId = "58901235",
                FirstMidName = "Justin",
                LastName = "Lewis",
                TestEnrollmentDate = DateOnly.Parse("2004-12-19"),
            },
            new TestStudent
            {
                GovernmentId = "42345680",
                FirstMidName = "Lizabeth",
                LastName = "Lee",
                TestEnrollmentDate = DateOnly.Parse("2014-07-02"),
            },
            new TestStudent
            {
                GovernmentId = "55678904",
                FirstMidName = "Kevin",
                LastName = "Walker",
                TestEnrollmentDate = DateOnly.Parse("2000-02-14"),
            },
            new TestStudent
            {
                GovernmentId = "49012347",
                FirstMidName = "Heather",
                LastName = "Hall",
                TestEnrollmentDate = DateOnly.Parse("2009-10-28"),
            },
            new TestStudent
            {
                GovernmentId = "53456782",
                FirstMidName = "Brian",
                LastName = "Allen",
                TestEnrollmentDate = DateOnly.Parse("2022-05-11"),
            },
            new TestStudent
            {
                GovernmentId = "46789015",
                FirstMidName = "Michelle",
                LastName = "Young",
                TestEnrollmentDate = DateOnly.Parse("1996-09-23"),
            },
            new TestStudent
            {
                GovernmentId = "40123458",
                FirstMidName = "Jason",
                LastName = "Hernandez",
                TestEnrollmentDate = DateOnly.Parse("2006-03-07"),
            },
            new TestStudent
            {
                GovernmentId = "54567894",
                FirstMidName = "Melissa",
                LastName = "King",
                TestEnrollmentDate = DateOnly.Parse("2015-11-20"),
            },
            new TestStudent
            {
                GovernmentId = "47890126",
                FirstMidName = "James",
                LastName = "Wright",
                TestEnrollmentDate = DateOnly.Parse("2001-08-04"),
            },
            new TestStudent
            {
                GovernmentId = "51234570",
                FirstMidName = "Rebecca",
                LastName = "Lopez",
                TestEnrollmentDate = DateOnly.Parse("2019-01-17"),
            },
            new TestStudent
            {
                GovernmentId = "45678905",
                FirstMidName = "Tyler",
                LastName = "Hill",
                TestEnrollmentDate = DateOnly.Parse("2010-06-29"),
            },
            new TestStudent
            {
                GovernmentId = "59012348",
                FirstMidName = "Amy",
                LastName = "Scott",
                TestEnrollmentDate = DateOnly.Parse("1998-12-12"),
            },
            new TestStudent
            {
                GovernmentId = "43456783",
                FirstMidName = "Jacob",
                LastName = "Green",
                TestEnrollmentDate = DateOnly.Parse("2023-04-25"),
            },
            new TestStudent
            {
                GovernmentId = "56789016",
                FirstMidName = "Rachel",
                LastName = "Adams",
                TestEnrollmentDate = DateOnly.Parse("2007-09-08"),
            },
            new TestStudent
            {
                GovernmentId = "50123459",
                FirstMidName = "Nicholas",
                LastName = "Baker",
                TestEnrollmentDate = DateOnly.Parse("2016-02-20"),
            },
            new TestStudent
            {
                GovernmentId = "44567895",
                FirstMidName = "Kimberly",
                LastName = "Gonzalez",
                TestEnrollmentDate = DateOnly.Parse("2003-07-14"),
            },
            new TestStudent
            {
                GovernmentId = "58901237",
                FirstMidName = "Jonathan",
                LastName = "Nelson",
                TestEnrollmentDate = DateOnly.Parse("2012-12-26"),
            },
            new TestStudent
            {
                GovernmentId = "42345682",
                FirstMidName = "Lauren",
                LastName = "Carter",
                TestEnrollmentDate = DateOnly.Parse("2000-05-09"),
            },
            new TestStudent
            {
                GovernmentId = "55678906",
                FirstMidName = "Brittany",
                LastName = "Perez",
                TestEnrollmentDate = DateOnly.Parse("1997-03-05"),
            },
            new TestStudent
            {
                GovernmentId = "49012349",
                FirstMidName = "Austin",
                LastName = "Roberts",
                TestEnrollmentDate = DateOnly.Parse("2008-08-18"),
            },
            new TestStudent
            {
                GovernmentId = "53456784",
                FirstMidName = "Victoria",
                LastName = "Turner",
                TestEnrollmentDate = DateOnly.Parse("2018-01-30"),
            },
            new TestStudent
            {
                GovernmentId = "46789017",
                FirstMidName = "Zachary",
                LastName = "Phillips",
                TestEnrollmentDate = DateOnly.Parse("2005-06-13"),
            },
            new TestStudent
            {
                GovernmentId = "40123460",
                FirstMidName = "Chris",
                LastName = "Campbell",
                TestEnrollmentDate = DateOnly.Parse("2013-11-26"),
            },
            new TestStudent
            {
                GovernmentId = "54567896",
                FirstMidName = "Ethan",
                LastName = "Parker",
                TestEnrollmentDate = DateOnly.Parse("2002-04-09"),
            },
            new TestStudent
            {
                GovernmentId = "47890128",
                FirstMidName = "Danielle",
                LastName = "Evans",
                TestEnrollmentDate = DateOnly.Parse("2020-09-21"),
            },
            new TestStudent
            {
                GovernmentId = "51234572",
                FirstMidName = "Nathan",
                LastName = "Edwards",
                TestEnrollmentDate = DateOnly.Parse("1999-02-03"),
            },
            new TestStudent
            {
                GovernmentId = "45678907",
                FirstMidName = "Amber",
                LastName = "Collins",
                TestEnrollmentDate = DateOnly.Parse("2009-07-17"),
            },
            new TestStudent
            {
                GovernmentId = "59012350",
                FirstMidName = "Kyle",
                LastName = "Stewart",
                TestEnrollmentDate = DateOnly.Parse("2017-12-30"),
            },
            new TestStudent
            {
                GovernmentId = "43456785",
                FirstMidName = "Alexis",
                LastName = "Sanchez",
                TestEnrollmentDate = DateOnly.Parse("2004-05-12"),
            },
            new TestStudent
            {
                GovernmentId = "56789018",
                FirstMidName = "Cody",
                LastName = "Morris",
                TestEnrollmentDate = DateOnly.Parse("2014-10-25"),
            },
            new TestStudent
            {
                GovernmentId = "50123461",
                FirstMidName = "Courtney",
                LastName = "Rogers",
                TestEnrollmentDate = DateOnly.Parse("2001-03-08"),
            },
            new TestStudent
            {
                GovernmentId = "44567897",
                FirstMidName = "Aaron",
                LastName = "Reed",
                TestEnrollmentDate = DateOnly.Parse("2011-08-21"),
            },
            new TestStudent
            {
                GovernmentId = "58901239",
                FirstMidName = "Kayla",
                LastName = "Cook",
                TestEnrollmentDate = DateOnly.Parse("1998-01-04"),
            },
            new TestStudent
            {
                GovernmentId = "42345684",
                FirstMidName = "Jordan",
                LastName = "Morgan",
                TestEnrollmentDate = DateOnly.Parse("2019-06-17"),
            },
            new TestStudent
            {
                GovernmentId = "55678908",
                FirstMidName = "Kristen",
                LastName = "Bell",
                TestEnrollmentDate = DateOnly.Parse("2006-11-29"),
            },
            new TestStudent
            {
                GovernmentId = "49012351",
                FirstMidName = "Dylan",
                LastName = "Murphy",
                TestEnrollmentDate = DateOnly.Parse("2015-04-13"),
            },
            new TestStudent
            {
                GovernmentId = "53456786",
                FirstMidName = "Taylor",
                LastName = "Bailey",
                TestEnrollmentDate = DateOnly.Parse("2003-09-26"),
            },
            new TestStudent
            {
                GovernmentId = "46789019",
                FirstMidName = "Morgan",
                LastName = "Rivera",
                TestEnrollmentDate = DateOnly.Parse("2012-02-08"),
            },
            new TestStudent
            {
                GovernmentId = "40123462",
                FirstMidName = "Travis",
                LastName = "Cooper",
                TestEnrollmentDate = DateOnly.Parse("2000-07-22"),
            },
            new TestStudent
            {
                GovernmentId = "54567898",
                FirstMidName = "Hannah",
                LastName = "Richard",
                TestEnrollmentDate = DateOnly.Parse("2021-12-04"),
            },
            new TestStudent
            {
                GovernmentId = "47890130",
                FirstMidName = "Adam",
                LastName = "Cox",
                TestEnrollmentDate = DateOnly.Parse("1997-05-18"),
            },
            new TestStudent
            {
                GovernmentId = "51234574",
                FirstMidName = "Sydney",
                LastName = "Howard",
                TestEnrollmentDate = DateOnly.Parse("2008-10-31"),
            },
            new TestStudent
            {
                GovernmentId = "45678909",
                FirstMidName = "Lucas",
                LastName = "Ward",
                TestEnrollmentDate = DateOnly.Parse("2018-03-15"),
            },
            new TestStudent
            {
                GovernmentId = "59012352",
                FirstMidName = "Allison",
                LastName = "Torres",
                TestEnrollmentDate = DateOnly.Parse("2005-08-28"),
            },
            new TestStudent
            {
                GovernmentId = "43456787",
                FirstMidName = "Mason",
                LastName = "Peterson",
                TestEnrollmentDate = DateOnly.Parse("2013-01-10"),
            },
            new TestStudent
            {
                GovernmentId = "56789020",
                FirstMidName = "Olivia",
                LastName = "Gray",
                TestEnrollmentDate = DateOnly.Parse("2002-06-24"),
            },
            new TestStudent
            {
                GovernmentId = "50123463",
                FirstMidName = "Logan",
                LastName = "Ramirez",
                TestEnrollmentDate = DateOnly.Parse("2020-11-06"),
            },
            new TestStudent
            {
                GovernmentId = "44567899",
                FirstMidName = "Emma",
                LastName = "James",
                TestEnrollmentDate = DateOnly.Parse("1999-04-20"),
            },
            new TestStudent
            {
                GovernmentId = "58901241",
                FirstMidName = "Connor",
                LastName = "Watson",
                TestEnrollmentDate = DateOnly.Parse("2010-09-02"),
            },
            new TestStudent
            {
                GovernmentId = "42345686",
                FirstMidName = "Sophia",
                LastName = "Brooks",
                TestEnrollmentDate = DateOnly.Parse("2007-02-15"),
            },
            new TestStudent
            {
                GovernmentId = "55678910",
                FirstMidName = "Liam",
                LastName = "Kelly",
                TestEnrollmentDate = DateOnly.Parse("2016-07-29"),
            },
            new TestStudent
            {
                GovernmentId = "49012353",
                FirstMidName = "Isabella",
                LastName = "Sanders",
                TestEnrollmentDate = DateOnly.Parse("2004-12-11"),
            },
            new TestStudent
            {
                GovernmentId = "53456788",
                FirstMidName = "Noah",
                LastName = "Price",
                TestEnrollmentDate = DateOnly.Parse("2014-05-24"),
            },
            new TestStudent
            {
                GovernmentId = "46789021",
                FirstMidName = "Ava",
                LastName = "Bennett",
                TestEnrollmentDate = DateOnly.Parse("2001-10-07"),
            },
            new TestStudent
            {
                GovernmentId = "40123464",
                FirstMidName = "William",
                LastName = "Wood",
                TestEnrollmentDate = DateOnly.Parse("2011-03-21"),
            },
            new TestStudent
            {
                GovernmentId = "54567900",
                FirstMidName = "Mia",
                LastName = "Barnes",
                TestEnrollmentDate = DateOnly.Parse("1998-08-04"),
            },
            new TestStudent
            {
                GovernmentId = "47890132",
                FirstMidName = "Benjamin",
                LastName = "Ross",
                TestEnrollmentDate = DateOnly.Parse("2019-01-17"),
            },
            new TestStudent
            {
                GovernmentId = "51234576",
                FirstMidName = "Charl",
                LastName = "Henderson",
                TestEnrollmentDate = DateOnly.Parse("2006-06-30"),
            },
            new TestStudent
            {
                GovernmentId = "45678911",
                FirstMidName = "Elijah",
                LastName = "Coleman",
                TestEnrollmentDate = DateOnly.Parse("2015-12-13"),
            },
            new TestStudent
            {
                GovernmentId = "59012354",
                FirstMidName = "Amelia",
                LastName = "Jenkins",
                TestEnrollmentDate = DateOnly.Parse("2003-05-27"),
            },
            new TestStudent
            {
                GovernmentId = "43456789",
                FirstMidName = "Oliver",
                LastName = "Perry",
                TestEnrollmentDate = DateOnly.Parse("2012-10-09"),
            },
            new TestStudent
            {
                GovernmentId = "56789022",
                FirstMidName = "Harper",
                LastName = "Powell",
                TestEnrollmentDate = DateOnly.Parse("2000-03-23"),
            },
            new TestStudent
            {
                GovernmentId = "50123465",
                FirstMidName = "Henry",
                LastName = "Long",
                TestEnrollmentDate = DateOnly.Parse("2021-08-05"),
            },
            new TestStudent
            {
                GovernmentId = "44567901",
                FirstMidName = "Evelyn",
                LastName = "Patterson",
                TestEnrollmentDate = DateOnly.Parse("1997-01-19"),
            },
            new TestStudent
            {
                GovernmentId = "58901243",
                FirstMidName = "Alex",
                LastName = "Hughes",
                TestEnrollmentDate = DateOnly.Parse("2009-06-03"),
            },
            new TestStudent
            {
                GovernmentId = "42345688",
                FirstMidName = "Abigail",
                LastName = "Flores",
                TestEnrollmentDate = DateOnly.Parse("2017-11-16"),
            },
            new TestStudent
            {
                GovernmentId = "55678912",
                FirstMidName = "Sebas",
                LastName = "Washing",
                TestEnrollmentDate = DateOnly.Parse("2005-04-30"),
            },
            new TestStudent
            {
                GovernmentId = "49012355",
                FirstMidName = "Emily",
                LastName = "Butler",
                TestEnrollmentDate = DateOnly.Parse("2013-09-12"),
            },
            new TestStudent
            {
                GovernmentId = "53456790",
                FirstMidName = "Jack",
                LastName = "Simmons",
                TestEnrollmentDate = DateOnly.Parse("2002-02-26"),
            },
            new TestStudent
            {
                GovernmentId = "46789023",
                FirstMidName = "Ella",
                LastName = "Foster",
                TestEnrollmentDate = DateOnly.Parse("2020-07-10"),
            },
            new TestStudent
            {
                GovernmentId = "40123466",
                FirstMidName = "AIden",
                LastName = "Gonzales",
                TestEnrollmentDate = DateOnly.Parse("1999-12-23"),
            },
        ];
    }
}
