namespace Perfectial.UnitTests.Persistence
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using FizzWare.NBuilder;

    using NUnit.Framework;

    using Perfectial.Domain.Model;
    using Perfectial.Infrastructure.Persistence;
    using Perfectial.Infrastructure.Persistence.Base;
    using Perfectial.Infrastructure.Persistence.EntityFramework;

    [TestFixture]
    public class OneToManyRelationshipRepositoryTest
    {
        private const int NumberOfCourses = 10;
        private const int NumberOfStandards = 10;
        private const int NumberOfTeachers = 10;

        private IAmbientDbContextLocator ambientDbContextLocator;
        private IDbContextScopeFactory dbContextScopeFactory;

        private Repository<ApplicationDbContext, Student, int> studentRepository;
        private Repository<ApplicationDbContext, Teacher, int> teacherRepository;
        private Repository<ApplicationDbContext, Standard, int> standardRepository;
        private Repository<ApplicationDbContext, Course, int> courseRepository;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            this.ambientDbContextLocator = new AmbientDbContextLocator();
            this.dbContextScopeFactory = new DbContextScopeFactory();

            this.studentRepository = new Repository<ApplicationDbContext, Student, int>(this.ambientDbContextLocator);
            this.teacherRepository = new Repository<ApplicationDbContext, Teacher, int>(this.ambientDbContextLocator);
            this.standardRepository = new Repository<ApplicationDbContext, Standard, int>(this.ambientDbContextLocator);
            this.courseRepository = new Repository<ApplicationDbContext, Course, int>(this.ambientDbContextLocator);

            Database.SetInitializer(new ApplicationDbInitializer());
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                var dbContext = dbContextScope.DbContexts.Get<ApplicationDbContext>();
                dbContext.Database.Initialize(true);
            }
        }

        [TestFixtureTearDown]
        public void TestCleanup()
        {
        }

        [Test]
        public void ShouldAddEntityWithReferencesToExistingEntitiesWithoutDuplicates()
        {
            var originalStandardName = Faker.Name.FullName();
            var originalStandards = this.AddStandards(originalStandardName);
            var originalTeacherName = Faker.Name.FullName();
            var originalTeachers = this.AddTeachers(originalStandards, originalTeacherName);
            var originalCourseName = Faker.Name.FullName();
            var originalCourses = this.AddCourses(originalTeachers, originalCourseName);
            this.AddStudent(originalStandards, originalCourses);

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualStandards = this.standardRepository.GetAllList(standard => standard.Name == originalStandardName);

                Assert.IsTrue(actualStandards.Any());
                Assert.AreEqual(originalStandards.Count, actualStandards.Count);

                var actualTeachers = this.teacherRepository.GetAllList(teacher => teacher.Name == originalTeacherName);

                Assert.IsTrue(actualTeachers.Any());
                Assert.AreEqual(originalTeachers.Count, actualTeachers.Count);

                var actualCourses = this.courseRepository.GetAllList(course => course.Name == originalCourseName);

                Assert.IsTrue(actualCourses.Any());
                Assert.AreEqual(originalCourses.Count, actualCourses.Count);
            }
        }

        [Test]
        public void ShouldUpdateEntityWithReferencesToExistingEntitiesWithoutDuplicates()
        {
            var originalStandardName = Faker.Name.FullName();
            var originalStandards = this.AddStandards(originalStandardName);
            var originalTeacherName = Faker.Name.FullName();
            var originalTeachers = this.AddTeachers(originalStandards, originalTeacherName);
            var originalCourseName = Faker.Lorem.GetFirstWord();
            var originalCourses = this.AddCourses(originalTeachers, originalCourseName);
            var originalStudent = this.AddStudent(originalStandards, originalCourses);

            var updatedStudentDescription = Faker.Name.FullName();
            var updatedCourseDescription = Faker.Lorem.GetFirstWord();
            var updatedTeacherDescription = Faker.Name.FullName();
            var updatedStandardDescription = Faker.Name.FullName();

            Student actualStudent;
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualStudent = this.studentRepository.Get(originalStudent.Id);

                Assert.IsNotNull(actualStudent);
                Assert.AreEqual(originalStudent, actualStudent);

                actualStudent.Description = updatedStudentDescription;
                this.studentRepository.Update(actualStudent);

                foreach (var actualCourse in actualStudent.Courses)
                {
                    actualCourse.Description = updatedCourseDescription;
                    actualCourse.Teacher.Description = updatedTeacherDescription;
                    actualCourse.Teacher.Standard.Description = updatedStandardDescription;
                }

                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualStudent = this.studentRepository.Get(originalStudent.Id);

                Assert.IsNotNull(actualStudent);

                Assert.AreNotEqual(originalStudent, actualStudent);
                Assert.AreEqual(updatedStudentDescription, actualStudent.Description);

                foreach (var actualCourse in actualStudent.Courses)
                {
                    Assert.AreEqual(updatedCourseDescription, actualCourse.Description);
                    Assert.AreEqual(updatedTeacherDescription, actualCourse.Teacher.Description);
                    Assert.AreEqual(updatedStandardDescription, actualCourse.Teacher.Standard.Description);
                }
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualStandards = this.standardRepository.GetAllList(standard => standard.Name == originalStandardName);

                Assert.IsTrue(actualStandards.Any());
                Assert.AreEqual(originalStandards.Count, actualStandards.Count);

                var actualTeachers = this.teacherRepository.GetAllList(teacher => teacher.Name == originalTeacherName);

                Assert.IsTrue(actualTeachers.Any());
                Assert.AreEqual(originalTeachers.Count, actualTeachers.Count);

                var actualCourses = this.courseRepository.GetAllList(course => course.Name == originalCourseName);

                Assert.IsTrue(actualCourses.Any());
                Assert.AreEqual(originalCourses.Count, actualCourses.Count);
            }
        }

        private IList<Standard> CreateStandards(int numberOfStandards, string name)
        {
            var standards = Builder<Standard>.CreateListOfSize(numberOfStandards)
                .All()
                    .With(e => e.Id = 0)
                    .With(e => e.Name = name)
                    .With(e => e.Description = Faker.Lorem.Sentence())
                .Build().ToList();

            return standards;
        }

        private IList<Standard> AddStandards(string name)
        {
            var originalStandards = this.CreateStandards(NumberOfStandards, name);
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.standardRepository.AddRange(originalStandards.ToArray());
                dbContextScope.SaveChanges();
            }

            return originalStandards;
        }

        private IList<Teacher> CreateTeachers(int numberOfTeachers, IList<Standard> standards, string name)
        {
            var teachers = Builder<Teacher>.CreateListOfSize(numberOfTeachers)
                .All()
                    .With(e => e.Id = 0)
                    .With(e => e.Name = name)
                    .With(e => e.Description = Faker.Lorem.Sentence())
                    .With(e => e.Type = 0)
                    .With(e => e.Standard = Pick<Standard>.RandomItemFrom(standards))
                .Build().ToList();

            teachers.ForEach(teacher => teacher.StandardId = teacher.Standard.Id);

            return teachers;
        }

        private IList<Teacher> AddTeachers(IList<Standard> standards, string name)
        {
            var originalTeachers = this.CreateTeachers(NumberOfTeachers, standards, name);
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.teacherRepository.AddRange(originalTeachers.ToArray());
                dbContextScope.SaveChanges();
            }

            return originalTeachers;
        }

        private IList<Course> CreateCourses(int numberOfCourses, IList<Teacher> teachers, string name)
        {
            var courses = Builder<Course>.CreateListOfSize(numberOfCourses)
                .All()
                    .With(e => e.Id = 0)
                    .With(e => e.Name = name)
                    .With(e => e.Description = Faker.Lorem.Sentence())
                    .With(e => e.Teacher = Pick<Teacher>.RandomItemFrom(teachers))
                .Build().ToList();

            courses.ForEach(сourse => сourse.TeacherId = сourse.Teacher.Id);

            return courses;
        }

        private IList<Course> AddCourses(IList<Teacher> teachers, string name)
        {
            var originalCourses = this.CreateCourses(NumberOfCourses, teachers, name);
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.courseRepository.AddRange(originalCourses.ToArray());
                dbContextScope.SaveChanges();
            }

            return originalCourses;
        }

        private Student CreateStudent(IList<Standard> standards, IList<Course> courses)
        {
            var student = Builder<Student>.CreateNew()
                .With(e => e.Id = 0)
                .With(e => e.Name = Faker.Name.FullName())
                .With(e => e.Description = Faker.Lorem.Sentence())
                .With(e => e.StudentAddress = this.CreateStudentAddress())
                .With(e => e.Standard = Pick<Standard>.RandomItemFrom(standards))
                .With(e => e.Courses = new List<Course>(courses))
                .Build();

            student.StandardId = student.Standard.Id;

            return student;
        }

        private Student AddStudent(IList<Standard> standards, IList<Course> courses)
        {
            var originalStudent = this.CreateStudent(standards, courses);
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.studentRepository.Add(originalStudent);
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalStudent.Id != 0);
            }

            return originalStudent;
        }

        private StudentAddress CreateStudentAddress()
        {
            var student = Builder<StudentAddress>.CreateNew()
                .With(e => e.Id = 0)
                .With(e => e.Address1 = Faker.Address.StreetAddress())
                .With(e => e.Address2 = Faker.Address.SecondaryAddress())
                .With(e => e.City = Faker.Address.City())
                .With(e => e.State = Faker.Address.UsState())
                .With(e => e.State = Faker.Address.UsState())
                .With(e => e.Student = null)
                .Build();

            return student;
        }
    }
}
