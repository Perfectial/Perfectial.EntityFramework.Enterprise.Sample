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
    public class OneToOneRelationshipRepositoryTest
    {
        private const int NumberOfStudensToCreate = 10;

        private IAmbientDbContextLocator ambientDbContextLocator;
        private IDbContextScopeFactory dbContextScopeFactory;

        private Repository<ApplicationDbContext, Student, int> studentRepository;

        [TestFixtureSetUp]
        public void TestSetup()
        {
            this.ambientDbContextLocator = new AmbientDbContextLocator();
            this.dbContextScopeFactory = new DbContextScopeFactory();

            this.studentRepository = new Repository<ApplicationDbContext, Student, int>(this.ambientDbContextLocator);

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
        public void ShouldGetAllStudents()
        {
            this.AddStudents();

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualEntities = this.studentRepository.GetAll();
                Assert.IsTrue(actualEntities.Any());
            }
        }

        [Test]
        public void ShouldGetStudentById()
        {
            var originalStudent = this.AddStudent();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualStudent = this.studentRepository.Get(originalStudent.Id);

                Assert.IsNotNull(actualStudent);
                Assert.AreEqual(originalStudent, actualStudent);
            }
        }

        [Test]
        public void ShouldAddStudent()
        {
            var originalStudent = this.AddStudent();
            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                var actualStudent = this.studentRepository.Get(originalStudent.Id);

                Assert.IsNotNull(actualStudent);
                Assert.AreEqual(originalStudent, actualStudent);
            }
        }

        [Test]
        public void ShouldAddOrUpdateStudent()
        {
            var originalEntity = this.CreateStudent();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.studentRepository.AddOrUpdate(originalEntity);
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalEntity.Id != 0);
            }

            Student actualStudent;
            var studentName = Faker.Name.FullName();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualStudent = this.studentRepository.Get(originalEntity.Id);

                Assert.IsNotNull(actualStudent);
                Assert.AreEqual(originalEntity, actualStudent);

                actualStudent.Name = studentName;
                this.studentRepository.AddOrUpdate(actualStudent);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualStudent = this.studentRepository.Get(originalEntity.Id);

                Assert.IsNotNull(actualStudent);
                Assert.AreNotEqual(originalEntity, actualStudent);
                Assert.AreEqual(studentName, actualStudent.Name);
            }
        }

        [Test]
        public void ShouldUpdateStudent()
        {
            var originalEntity = this.AddStudent();

            Student actualStudent;
            var studentName = Faker.Name.FullName();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualStudent = this.studentRepository.Get(originalEntity.Id);

                Assert.IsNotNull(actualStudent);
                Assert.AreEqual(originalEntity, actualStudent);

                actualStudent.Name = studentName;
                this.studentRepository.Update(actualStudent);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualStudent = this.studentRepository.Get(originalEntity.Id);

                Assert.IsNotNull(actualStudent);
                Assert.AreNotEqual(originalEntity, actualStudent);
                Assert.AreEqual(studentName, actualStudent.Name);
            }
        }

        [Test]
        public void ShouldDeleteStudent()
        {
            var originalEntity = this.AddStudent();

            Student actualStudent;
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                actualStudent = this.studentRepository.Get(originalEntity.Id);

                Assert.IsNotNull(actualStudent);
                Assert.AreEqual(originalEntity, actualStudent);

                this.studentRepository.Delete(actualStudent);
                dbContextScope.SaveChanges();
            }

            using (this.dbContextScopeFactory.CreateReadOnly())
            {
                actualStudent = this.studentRepository.FirstOrDefault(originalEntity.Id);

                Assert.IsNull(actualStudent);
            }
        }

        private Student CreateStudent()
        {
            var student = Builder<Student>.CreateNew()
                .With(e => e.Id = 0)
                .With(e => e.Name = Faker.Name.FullName())
                .With(e => e.StudentAddress = this.CreateStudentAddress())
                .With(e => e.StandardId = null)
                .With(e => e.Standard = null)
                .With(e => e.Courses = new List<Course>())
                .Build();

            return student;
        }

        private List<Student> CreateStudents(int numberOfStudents)
        {
            var students = Builder<Student>.CreateListOfSize(numberOfStudents)
                .All()
                    .With(e => e.Id = 0)
                    .With(e => e.Name = Faker.Name.FullName())
                    .With(e => e.StudentAddress = this.CreateStudentAddress())
                    .With(e => e.StandardId = null)
                    .With(e => e.Standard = null)
                    .With(e => e.Courses = new List<Course>())
                .Build().ToList();

            return students;
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

        private Student AddStudent()
        {
            var originalStudent = this.CreateStudent();
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.studentRepository.Add(originalStudent);
                dbContextScope.SaveChanges();

                Assert.IsTrue(originalStudent.Id != 0);
            }

            return originalStudent;
        }

        private void AddStudents()
        {
            var originalStudents = this.CreateStudents(NumberOfStudensToCreate);
            using (var dbContextScope = this.dbContextScopeFactory.Create())
            {
                this.studentRepository.AddRange(originalStudents.ToArray());
                dbContextScope.SaveChanges();
            }
        }
    }
}
