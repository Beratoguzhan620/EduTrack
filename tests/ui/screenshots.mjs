// Visual smoke test: logs in with the seeded admin, visits the main screens and saves screenshots.
import { chromium } from "playwright";
import { mkdir } from "node:fs/promises";

const baseUrl = process.env.BASE_URL ?? "http://localhost:5080";
const out = new URL("./output/", import.meta.url).pathname;
const admin = { email: "admin@example.com", password: "112" };

const viewports = {
  desktop: { width: 1440, height: 900 },
  mobile: { width: 390, height: 844 },
};

await mkdir(out, { recursive: true });
const browser = await chromium.launch();

async function shot(page, name) {
  await page.waitForLoadState("networkidle").catch(() => {});
  await page.screenshot({ path: `${out}${name}.png`, fullPage: true });
  console.log(`saved ${name}.png`);
}

async function login(page) {
  await page.goto(`${baseUrl}/Login`);
  await page.fill("#Email", admin.email);
  await page.fill("#passwordInput", admin.password);
  await Promise.all([page.waitForNavigation(), page.click("button[type=submit]")]);
}

for (const [device, viewport] of Object.entries(viewports)) {
  const context = await browser.newContext({ viewport, locale: "tr-TR", deviceScaleFactor: device === "mobile" ? 2 : 1 });
  const page = await context.newPage();

  await page.goto(`${baseUrl}/Login`);
  await shot(page, `${device}-login`);

  await page.click("button[type=submit]");
  await shot(page, `${device}-login-validation`);

  await page.goto(`${baseUrl}/Register`);
  await shot(page, `${device}-register`);

  await page.click("button[type=submit]");
  await shot(page, `${device}-register-validation`);

  await login(page);
  await page.goto(`${baseUrl}/Course`);
  await shot(page, `${device}-courses`);

  if (device === "desktop") {
    // Create a course through the modal so the grid has content.
    await page.click("button[data-bs-target='#createCourse']");
    await page.fill("#CreateCourseViewModel_Title", "5-A Matematik");
    await page.fill("#CreateCourseViewModel_Description", "Kesirler, ondalık gösterim ve problem çözme.");
    await Promise.all([page.waitForNavigation(), page.click("#createCourse button[type=submit]")]);
    await page.goto(`${baseUrl}/Course`);
    await shot(page, `${device}-courses-grid`);
  }

  await page.goto(`${baseUrl}/Users`);
  await shot(page, `${device}-users`);

  if (device === "mobile") {
    await page.click("button[data-bs-target='#appSidebar']");
    await page.waitForTimeout(400);
    await page.screenshot({ path: `${out}${device}-menu.png` });
    console.log(`saved ${device}-menu.png`);
  }

  await context.close();
}

// End-to-end assignment flow: instructor publishes, a new student registers,
// verifies the e-mail, joins the course and submits; the instructor grades it.
{
  const desktop = { viewport: viewports.desktop, locale: "tr-TR" };
  const teacher = await (await browser.newContext(desktop)).newPage();
  await login(teacher);

  await teacher.goto(`${baseUrl}/Course`);
  await Promise.all([teacher.waitForNavigation(), teacher.click("a[href*='/Course/Detail/'] >> nth=0")]);
  const courseUrl = teacher.url();
  const courseId = courseUrl.split("/").pop();
  const courseCode = (await teacher.textContent(".course-code-value")).trim();

  await teacher.goto(`${baseUrl}/Course/Assignments/${courseId}`);
  await shot(teacher, "desktop-assignments-empty");

  await teacher.click("button[data-bs-target='#createAssignment']");
  await teacher.waitForTimeout(300);
  await teacher.fill("#NewAssignment_Title", "Kesirlerle problem çözme");
  await teacher.fill("#NewAssignment_Instructions", "Ders kitabındaki 4. ünite sonundaki 1-10 arası soruları çözün.\nÇözüm adımlarını açıkça yazın.");
  await shot(teacher, "desktop-assignment-create");
  // Publishing opens the new assignment.
  await Promise.all([teacher.waitForNavigation(), teacher.click("#createAssignment button[type=submit]")]);
  const assignmentUrl = teacher.url();
  await teacher.goto(`${baseUrl}/Course/Assignments/${courseId}`);
  await shot(teacher, "desktop-assignments-instructor");

  const studentContext = await browser.newContext(desktop);
  const student = await studentContext.newPage();
  await student.goto(`${baseUrl}/Register`);
  await student.fill("#FirstName", "Ayşe");
  await student.fill("#LastName", "Yılmaz");
  await student.fill("#Email", "ayse.yilmaz@example.com");
  await student.fill("#registerPassword", "Demo123!");
  await Promise.all([student.waitForNavigation(), student.click("button[type=submit]")]);
  await shot(student, "desktop-verify-email");

  const code = (await student.textContent(".alert strong.font-mono")).trim();
  await student.fill("#Code", code);
  await Promise.all([student.waitForNavigation(), student.click("form[action$='VerifyEmail'] button[type=submit]")]);
  await shot(student, "desktop-login-after-verify");

  await student.fill("#Email", "ayse.yilmaz@example.com");
  await student.fill("#passwordInput", "Demo123!");
  await Promise.all([student.waitForNavigation(), student.click("button[type=submit]")]);
  await student.goto(`${baseUrl}/Course`);
  await student.click("button[data-bs-target='#joinCourse']");
  await student.waitForTimeout(300);
  await student.fill("#JoinCourseViewModel_Code", courseCode);
  await Promise.all([student.waitForNavigation(), student.click("#joinCourse button[type=submit]")]);
  await shot(student, "desktop-joined-course");

  await student.goto(`${baseUrl}/Course/Detail/${courseId}`);
  await shot(student, "desktop-course-stream-student");
  await student.goto(`${baseUrl}/Course/Assignments/${courseId}`);
  await shot(student, "desktop-assignments-student");

  await student.goto(assignmentUrl);
  await shot(student, "desktop-assignment-student-open");
  await student.fill("#Submission_Text", "1) 3/4 + 1/8 = 7/8\n2) 2/3 x 9 = 6\n3) 5/6 - 1/3 = 1/2");
  await Promise.all([student.waitForNavigation(), student.click("form[action*='Submit'] button[type=submit]")]);
  await shot(student, "desktop-assignment-student-submitted");

  await teacher.goto(assignmentUrl);
  await teacher.click("button:has-text('Notlandır')");
  await teacher.waitForTimeout(400);
  await teacher.fill("input[name=Grade]", "90");
  await teacher.fill("textarea[name=Feedback]", "Adımlar düzenli. 3. soruda sadeleştirmeyi unutmayın.");
  await shot(teacher, "desktop-assignment-grading");
  await Promise.all([teacher.waitForNavigation(), teacher.click("form[action*='Grade'] button[type=submit]")]);
  await shot(teacher, "desktop-assignment-instructor");

  await student.goto(assignmentUrl);
  await shot(student, "desktop-assignment-student-graded");

  const mobile = await browser.newContext({ viewport: viewports.mobile, locale: "tr-TR", deviceScaleFactor: 2, storageState: await studentContext.storageState() });
  const mobilePage = await mobile.newPage();
  await mobilePage.goto(assignmentUrl);
  await shot(mobilePage, "mobile-assignment-student");
  await mobilePage.goto(`${baseUrl}/Course/Assignments/${courseId}`);
  await shot(mobilePage, "mobile-assignments-student");
}

// Demo classroom (Seed:DemoContent): the seeded instructor and one of the students.
for (const [device, viewport] of Object.entries(viewports)) {
  const context = await browser.newContext({ viewport, locale: "tr-TR", deviceScaleFactor: device === "mobile" ? 2 : 1 });
  const page = await context.newPage();

  await page.goto(`${baseUrl}/Login`);
  await page.fill("#Email", "ogretmen@example.com");
  await page.fill("#passwordInput", "Demo123!");
  await Promise.all([page.waitForNavigation(), page.click("button[type=submit]")]);
  await page.goto(`${baseUrl}/Course`);
  await shot(page, `${device}-demo-courses`);

  await Promise.all([page.waitForNavigation(), page.locator(".card", { hasText: "5-A Matematik" }).locator("a[href*='/Course/Detail/']").first().click()]);
  const courseId = page.url().split("/").pop();
  await shot(page, `${device}-demo-stream`);
  await page.goto(`${baseUrl}/Course/Materials/${courseId}`);
  await shot(page, `${device}-demo-materials`);
  await page.goto(`${baseUrl}/Course/Students/${courseId}`);
  await shot(page, `${device}-demo-students`);
  await page.goto(`${baseUrl}/Course/Assignments/${courseId}`);
  await shot(page, `${device}-demo-assignments`);
  await page.goto(`${baseUrl}/Materials/Add/${courseId}`);
  await shot(page, `${device}-demo-material-add`);

  if (device === "desktop") {
    await page.goto(`${baseUrl}/Users`);
    await shot(page, `${device}-demo-access-denied`);
    await page.goto(`${baseUrl}/Course/Detail/00000000-0000-0000-0000-000000000000`);
    await shot(page, `${device}-demo-course-not-member`);
    await page.goto(`${baseUrl}/bu-sayfa-yok`);
    await shot(page, `${device}-demo-not-found`);
  }

  await context.close();
}

await browser.close();
