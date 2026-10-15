// Regenerates the images used in the README (docs/screenshots) from the demo classroom.
// Needs the app running with Seed:DemoContent=true. The walkthrough video is saved as
// output/media/walkthrough.webm; CI turns it into docs/screenshots/demo.gif with ffmpeg.
import { chromium } from "playwright";
import { mkdir, readdir, rename } from "node:fs/promises";

const baseUrl = process.env.BASE_URL ?? "http://localhost:5080";
const out = new URL("./output/media/", import.meta.url).pathname;
const teacher = { email: "ogretmen@example.com", password: "Demo123!" };
const student = { email: "zeynep@example.com", password: "Demo123!" };
const pause = (ms) => new Promise((resolve) => setTimeout(resolve, ms));

await mkdir(out, { recursive: true });
const browser = await chromium.launch();

async function login(page, user) {
  await page.goto(`${baseUrl}/Login`);
  await page.fill("#Email", user.email);
  await page.fill("#passwordInput", user.password);
  await Promise.all([page.waitForNavigation(), page.click("button[type=submit]")]);
}

async function shot(page, name, fullPage = false) {
  await page.waitForLoadState("networkidle").catch(() => {});
  await page.screenshot({ path: `${out}${name}.png`, fullPage });
  console.log(`saved ${name}.png`);
}

async function openCourse(page, title) {
  await page.goto(`${baseUrl}/Course`);
  await Promise.all([
    page.waitForNavigation(),
    page.locator(".card", { hasText: title }).locator("a[href*='/Course/Detail/']").first().click(),
  ]);
  return page.url().split("/").pop();
}

// Still images, desktop.
{
  const context = await browser.newContext({ viewport: { width: 1280, height: 800 }, locale: "tr-TR" });
  const page = await context.newPage();

  await page.goto(`${baseUrl}/Login`);
  await shot(page, "login");

  await login(page, teacher);
  await page.goto(`${baseUrl}/Course`);
  await shot(page, "courses");

  const courseId = await openCourse(page, "5-A Matematik");
  await shot(page, "stream");
  await page.goto(`${baseUrl}/Course/Materials/${courseId}`);
  await shot(page, "materials");
  await page.goto(`${baseUrl}/Course/Students/${courseId}`);
  await shot(page, "students");
  await page.goto(`${baseUrl}/Course/Assignments/${courseId}`);
  await shot(page, "assignments");

  await Promise.all([page.waitForNavigation(), page.click("a.btn[href^='/Assignments/'] >> nth=0")]);
  await shot(page, "assignment-detail");

  await context.close();
}

// Still image, phone.
{
  const context = await browser.newContext({ viewport: { width: 390, height: 844 }, deviceScaleFactor: 2, isMobile: true, hasTouch: true, locale: "tr-TR" });
  const page = await context.newPage();
  await login(page, student);
  await openCourse(page, "5-A Matematik");
  await shot(page, "mobile-stream");
  await context.close();
}

// Walkthrough video for the README GIF.
{
  const context = await browser.newContext({
    viewport: { width: 1280, height: 800 },
    locale: "tr-TR",
    recordVideo: { dir: out, size: { width: 1280, height: 800 } },
  });
  const page = await context.newPage();

  await page.goto(`${baseUrl}/Login`);
  await pause(600);
  await page.type("#Email", teacher.email, { delay: 25 });
  await page.type("#passwordInput", teacher.password, { delay: 25 });
  await pause(300);
  await Promise.all([page.waitForNavigation(), page.click("button[type=submit]")]);
  await page.goto(`${baseUrl}/Course`);
  await pause(1400);

  const courseId = await openCourse(page, "5-A Matematik");
  await pause(1600);
  await page.goto(`${baseUrl}/Course/Materials/${courseId}`);
  await pause(1400);
  await page.goto(`${baseUrl}/Course/Assignments/${courseId}`);
  await pause(1400);
  await Promise.all([page.waitForNavigation(), page.click("a.btn[href^='/Assignments/'] >> nth=0")]);
  await pause(1600);
  await page.mouse.wheel(0, 500);
  await pause(1200);
  await page.goto(`${baseUrl}/Course/Students/${courseId}`);
  await pause(1600);

  await context.close();

  const video = (await readdir(out)).find((name) => name.endsWith(".webm"));
  if (video) await rename(`${out}${video}`, `${out}walkthrough.webm`);
}

await browser.close();
