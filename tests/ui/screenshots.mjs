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

await browser.close();
