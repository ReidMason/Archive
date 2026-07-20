from utils import create_logger
from selenium import webdriver
from selenium.webdriver.support.wait import WebDriverWait
from selenium.webdriver.support import expected_conditions as ec
from selenium.webdriver.common.by import By
from selenium.webdriver.remote.webelement import WebElement


class Driver:
    logger = create_logger(__name__)

    def __init__(self, webdriver_path: str, fullscreen: bool = False, headless: bool = False):
        self.logger.debug('Webdriver starting')
        options = webdriver.FirefoxOptions()
        options.add_argument("--log-level=3")

        if headless:
            options.add_argument('--headless')

        self.driver = webdriver.Firefox(executable_path=webdriver_path, options=options)
        self.wait = WebDriverWait(self.driver, 30)

        if fullscreen:
            self.driver.fullscreen_window()

        self.logger.info('Webdriver started')

    def go(self, url: str):
        self.driver.get(url)

    def wait_for(self, css_selector: str):
        self.wait.until(ec.element_to_be_clickable((By.CSS_SELECTOR, css_selector)))

    def find_element(self, css_selector: str) -> WebElement:
        self.wait_for(css_selector)
        return self.driver.find_element(By.CSS_SELECTOR, css_selector)

    def click(self, css_selector: str):
        self.find_element(css_selector).click()

    def quit(self):
        self.driver.quit()

    def __enter__(self):
        return self

    def __exit__(self, type, value, traceback):
        self.logger.info('Driver quitting')
        self.quit()